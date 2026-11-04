# Contributing to SnapDrop

Thanks for helping! SnapDrop is a small Windows tray app; keeping it small and simple is a goal.

## Getting set up

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Clone the repo and run:
   ```powershell
   dotnet run --project src\SnapDrop
   ```
3. To build the installer too, install [Inno Setup 6](https://jrsoftware.org/isinfo.php) and run `scripts\build.ps1`.

Developer switches (see `SnapApp.RunArgs`): `--settings`, `--bar`, `--capture screen|region|window`, `--edit <file>`, `--thumb <file>`.

## Code map

| File | What it holds |
|---|---|
| `Program.cs`, `SnapApp.cs` | startup, single instance, tray icon, global hotkeys, capture flow |
| `Picker.cs` | the full-screen overlay for choosing an area or a window |
| `Thumb.cs` | the floating thumbnail, drag-out, paste-path-into-terminal |
| `Editor.cs` | crop and annotate window |
| `ScreenshotStore.cs` | where screenshots are saved ("smart" vs "always") |
| `Settings.cs`, `SettingsWindow.cs`, `Hotkeys.cs` | preferences and their UI |
| `Xaml.cs` | all window layouts and styles, as XAML strings parsed at runtime |
| `Native.cs`, `Util.cs` | Win32 interop and helpers |

## Guidelines

- Keep the app dependency-free (only the .NET SDK libraries).
- Test on a high-DPI screen and, if you can, a second monitor: coordinates are in physical pixels.
- Never delete or overwrite anything in the user's screenshots folder except through `ScreenshotStore`.
- Regenerate the icon and installer images with `tools\make-assets.ps1` instead of editing the bitmaps by hand.
- Describe what you changed and how you tested it in the pull request.

## Reporting bugs

Please include your Windows version, whether the app runs as administrator, what you expected and what happened. The log is at `%APPDATA%\SnapDrop\error.log`.
