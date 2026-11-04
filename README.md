<p align="center">
  <img src="assets/logo.png" width="112" alt="SnapDrop logo">
</p>

<h1 align="center">SnapDrop</h1>

<p align="center">
  <b>Screenshots you can drag anywhere.</b><br>
  A macOS-style screenshot tool for Windows: capture, crop, then drag the floating thumbnail straight into a chat, document, folder or terminal.
</p>

---

## What it does

Press **Ctrl+Shift+S**, drag over the area you want, and a thumbnail slides in at the corner of your screen, just like on a Mac.

- **Drag it anywhere.** Drop it into Slack, Discord, WhatsApp Web, Word, Photoshop, a folder, the desktop... no saving, no copy-paste.
- **Drop it on a terminal** (Windows Terminal, the Claude Code CLI, ...) and the file path is typed for you.
- **Click it** to crop and mark it up: arrow, box, text, highlighter, pen, and a pixelate tool for hiding sensitive info.
- **No clutter.** If you drag a screenshot somewhere, it lives *there*. If you don't, it is saved to your screenshots folder automatically.
- Also: full screen and window capture (press **Space** while selecting), timers, every shot copied to the clipboard, multiple monitors, high-DPI screens.

## Install

Download **`SnapDrop-Setup-x.y.z.exe`** from the [Releases](../../releases) page and run it. It installs in seconds and asks for no admin rights on a normal PC.

SnapDrop needs the **.NET 10 Desktop Runtime**. The installer downloads it for you if it is missing.

SnapDrop lives in the system tray. Click the tray icon to capture, right-click it for the menu, or open **Settings** from there.

## Shortcuts

All of these can be changed in **Settings → Shortcuts**.

| Keys | Action |
|---|---|
| `Ctrl+Shift+S` | Capture an area. Press `Space` to switch to *click a window*, `Esc` to cancel |
| `Ctrl+Shift+3` | Capture the full screen (the monitor your mouse is on) |
| `Ctrl+Shift+5` | Options bar: screen / window / selection, and a 3-5-10 second timer |
| `Print Screen` | Same as capture an area (optional) |

## Settings

Open them from the tray menu → **Settings…**

- **General**: start with Windows, screenshots folder, file name, PNG/JPEG, copy to clipboard, capture delay, and whether to keep a copy of dragged screenshots.
- **Shortcuts**: record your own keys for every action.
- **Thumbnail**: which corner, how long it stays, and whether dropping on a terminal pastes the file path.

Settings are stored in `%APPDATA%\SnapDrop\settings.ini`.

### How saving works

By default SnapDrop is *smart* about where a screenshot ends up:

1. The new screenshot waits in a temporary folder while its thumbnail is on screen.
2. **You drag it out** (into a folder, a chat, a terminal, ...): that is where it lives. Nothing is added to your screenshots folder.
3. **You don't**: it is copied into your screenshots folder (`Pictures\SnapDrop` by default) when the thumbnail goes away.

Prefer the classic behaviour? Set **Keep a copy in that folder** to *Always*.

## Build from source

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and, for the installer, [Inno Setup 6](https://jrsoftware.org/isinfo.php) (`winget install JRSoftware.InnoSetup`).

```powershell
powershell -ExecutionPolicy Bypass -File scripts\build.ps1
```

This produces `artifacts\publish\SnapDrop.exe` and `artifacts\SnapDrop-Setup-<version>.exe`. Add `-SkipInstaller` to build only the app, or `-Version 1.2.0` to set the version.

For quick development, run `dotnet run --project src\SnapDrop`. Developer switches: `--settings`, `--bar`, `--capture screen|region|window`, `--edit <file>`, `--thumb <file>`.

### Project layout

```
src/SnapDrop/      the app (C#, WPF + a little WinForms for the tray icon)
installer/         Inno Setup script and wizard artwork
assets/            icon and logo
tools/             make-assets.ps1 regenerates the icon and installer images
scripts/           build.ps1
```

## Troubleshooting

- **"Your Windows doesn't fully support CET"** when starting a .NET 10 app: some Windows 10 builds lack full CET support and .NET 10 enables it by default. SnapDrop already turns it off (`<CETCompat>false</CETCompat>`). Installing the latest Windows updates also fixes it.
- **A shortcut doesn't work**: another app may already own it. Settings shows which one failed. Pick different keys.
- **Dropping on Windows Terminal does nothing**: Windows Terminal can refuse file drops when it runs as administrator. SnapDrop detects this and pastes the file path for you (**Settings → Thumbnail**).
- Errors are logged to `%APPDATA%\SnapDrop\error.log`.

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)
