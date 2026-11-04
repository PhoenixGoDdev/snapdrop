; SnapDrop installer (Inno Setup 6).  Build it with:  scripts\build.ps1
; It installs for the current user (no admin rights needed) and offers to start SnapDrop with Windows.

#define MyAppName "SnapDrop"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "SnapDrop contributors"
#define MyAppURL "https://github.com/PhoenixGoDdev/snapdrop"
#define MyAppExeName "SnapDrop.exe"
#define DotNetUrl "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe"

[Setup]
AppId={{B6E1F2D4-6A0E-4C8B-9F3A-5D7C2E9A1B40}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
VersionInfoVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=auto
DisableWelcomePage=no
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=commandline
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts
OutputBaseFilename=SnapDrop-Setup-{#MyAppVersion}
SetupIconFile=..\assets\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
WizardStyle=modern
WizardImageFile=wizard-large.bmp,wizard-large@1.5x.bmp,wizard-large@2x.bmp
WizardSmallImageFile=wizard-small.bmp,wizard-small@1.5x.bmp,wizard-small@2x.bmp
Compression=lzma2/max
SolidCompression=yes
LicenseFile=..\LICENSE
CloseApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "Start SnapDrop when I sign in to Windows"; GroupDescription: "Options:"
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Options:"; Flags: unchecked

[Files]
Source: "..\artifacts\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "SnapDrop"; \
  ValueData: """{app}\{#MyAppExeName}"" --background"; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Open SnapDrop settings"; Flags: nowait postinstall skipifsilent unchecked; Parameters: "--settings"
Filename: "{app}\{#MyAppExeName}"; Description: "Start SnapDrop now"; Flags: nowait postinstall skipifsilent runasoriginaluser

[Code]
var
  DownloadPage: TDownloadWizardPage;

{ ---------- helpers ---------- }

procedure StopSnapDrop();
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

{ SnapDrop needs the .NET 10 Desktop Runtime. Look for it in the registry. }
function DotNetDesktopRuntimeInstalled(): Boolean;
var
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;
  if RegGetValueNames(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', Names) then
    for I := 0 to GetArrayLength(Names) - 1 do
      if Pos('10.', Names[I]) = 1 then
      begin
        Result := True;
        Exit;
      end;
end;

{ ---------- wizard ---------- }

procedure InitializeWizard();
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
end;

{ If the runtime is missing, download and install it before copying SnapDrop (Windows asks for permission). }
function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  if (CurPageID = wpReady) and not DotNetDesktopRuntimeInstalled() then
  begin
    DownloadPage.Clear;
    DownloadPage.Add('{#DotNetUrl}', 'windowsdesktop-runtime.exe', '');
    DownloadPage.Show;
    try
      try
        DownloadPage.Download;
        DownloadPage.SetText('Installing the .NET 10 Desktop Runtime...', '');
        if not ShellExec('runas', ExpandConstant('{tmp}\windowsdesktop-runtime.exe'), '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
           or ((ResultCode <> 0) and (ResultCode <> 3010)) then
        begin
          if SuppressibleMsgBox('The .NET 10 Desktop Runtime could not be installed automatically.'#13#10#13#10 +
               'SnapDrop will not start until it is installed. You can get it from https://dotnet.microsoft.com/download/dotnet/10.0'#13#10#13#10 +
               'Continue installing SnapDrop anyway?', mbError, MB_YESNO, IDNO) = IDNO then
            Result := False;
        end;
      except
        if DownloadPage.AbortedByUser then
          Log('Runtime download aborted by user.')
        else
          SuppressibleMsgBox('Could not download the .NET 10 Desktop Runtime:'#13#10 + GetExceptionMessage + #13#10#13#10 +
            'Install it from https://dotnet.microsoft.com/download/dotnet/10.0 and run this setup again.', mbCriticalError, MB_OK, IDOK);
        Result := False;
      end;
    finally
      DownloadPage.Hide;
    end;
  end;
end;

{ ---------- install / uninstall ---------- }

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  StopSnapDrop();   { so the old exe can be replaced }
  Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  { the "start with Windows" box was cleared: make sure an older autostart entry is gone }
  if (CurStep = ssPostInstall) and (not WizardIsTaskSelected('startup')) then
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'SnapDrop');
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    StopSnapDrop();
  if CurUninstallStep = usPostUninstall then
    { screenshots are never deleted; only the small settings folder is optional }
    if SuppressibleMsgBox('Also remove your SnapDrop settings?'#13#10#13#10 +
         'Your screenshots are not deleted either way.', mbConfirmation, MB_YESNO or MB_DEFBUTTON2, IDNO) = IDYES then
      DelTree(ExpandConstant('{userappdata}\SnapDrop'), True, True, True);
end;
