<#
.SYNOPSIS
  Builds SnapDrop and (if Inno Setup is installed) the installer.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts\build.ps1
  powershell -ExecutionPolicy Bypass -File scripts\build.ps1 -Version 1.2.0
  powershell -ExecutionPolicy Bypass -File scripts\build.ps1 -SkipInstaller

Output:  artifacts\publish\SnapDrop.exe   and   artifacts\SnapDrop-Setup-<version>.exe
Needs the .NET 10 SDK.  The installer step needs Inno Setup 6 (https://jrsoftware.org/isinfo.php).
#>
param(
    [string]$Version = "",
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$proj = Join-Path $root 'src\SnapDrop\SnapDrop.csproj'
$out  = Join-Path $root 'artifacts'

if (-not $Version) { $Version = ([xml](Get-Content $proj)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1 }
Write-Host "Building SnapDrop $Version" -ForegroundColor Cyan

Remove-Item (Join-Path $out 'publish') -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish $proj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:Version=$Version -o (Join-Path $out 'publish')
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }
Remove-Item (Join-Path $out 'publish\*.pdb') -Force -ErrorAction SilentlyContinue
Write-Host "`nApp:       $out\publish\SnapDrop.exe" -ForegroundColor Green

if ($SkipInstaller) { return }

$iscc = @(
    (Get-Command iscc.exe -ErrorAction SilentlyContinue).Source,
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $iscc) {
    Write-Warning "Inno Setup 6 not found, so no installer was built. Install it with:  winget install JRSoftware.InnoSetup"
    return
}

& $iscc "/DMyAppVersion=$Version" (Join-Path $root 'installer\SnapDrop.iss')
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup failed' }
Write-Host "`nInstaller: $out\SnapDrop-Setup-$Version.exe" -ForegroundColor Green
