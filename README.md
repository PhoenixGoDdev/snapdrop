# SnapDrop

A macOS-style screenshot tool for Windows.

Take a screenshot → a thumbnail slides in at the bottom-right → **drag it into any app** (chat, Word, Photoshop, a folder…) or click it to **crop / annotate**. No manual saving or copy-pasting.

## Run it
Double-click `dist\SnapDrop.exe`. It lives in the system tray (blue viewfinder icon).
Right-click the tray icon → **Start with Windows** to launch it at login.

## Shortcuts
| Keys | What |
|---|---|
| **Ctrl+Shift+S** | Capture an area (drag). **Space** switches to "click a window". **Esc** cancels |
| Ctrl+Shift+4 / PrintScreen | Same as above |
| Ctrl+Shift+3 | Full screen (the monitor your mouse is on) |
| Ctrl+Shift+5 | Options bar: entire screen / window / selection, 3-5-10 s timer |

## The thumbnail
- **Drag** it anywhere → drops the PNG file (and image data) into the target app.
- **Click** it → editor.  **✕** or right-click menu → copy / save as / show in folder / delete.
- It slides away after ~9 s (paused while your mouse is over it). Several stack up.
- Every shot is also **auto-saved** to `Pictures\SnapDrop` and **copied to the clipboard**.

## Editor
Crop (drag, adjust handles, Enter), Draw, Highlight, Arrow, Box, Text, Blur (pixelate), colours, sizes, undo/redo.
**Done** saves over the file and shows the thumbnail again. **⠿ Drag out** drags the edited image without touching the original file.

## Build
`powershell -File build.ps1` (needs the .NET 10 SDK) → `dist\SnapDrop.exe`.
Settings: `%APPDATA%\SnapDrop\settings.ini`. Errors: `%APPDATA%\SnapDrop\error.log`.

CET (hardware shadow stack) compatibility is disabled in the project (`<CETCompat>false</CETCompat>`), because .NET 10's default crashes at startup on some CET-enabled PCs.
