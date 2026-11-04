# Generates assets/banner.png (the README header). Run after tools\make-assets.ps1.
#   powershell -ExecutionPolicy Bypass -File tools\make-banner.ps1
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$CanvasW = 1280; $CanvasH = 420
$bmp = New-Object System.Drawing.Bitmap $CanvasW, $CanvasH
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.TextRenderingHint = 'ClearTypeGridFit'

# background: deep navy to indigo, with two soft glows
$bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush ((New-Object System.Drawing.Rectangle 0, 0, $CanvasW, $CanvasH), [System.Drawing.Color]::FromArgb(15, 18, 32), [System.Drawing.Color]::FromArgb(30, 41, 110), 20.0)
$g.FillRectangle($bg, 0, 0, $CanvasW, $CanvasH)
foreach ($c in @(@(250, 60, 640, 90), @(1100, 380, 700, 80))) {
    $pg = New-Object System.Drawing.Drawing2D.GraphicsPath; $pg.AddEllipse($c[0] - $c[2] / 2, $c[1] - $c[2] / 2, $c[2], $c[2])
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush $pg
    $pb.CenterColor = [System.Drawing.Color]::FromArgb($c[3], 96, 165, 250); $pb.SurroundColors = @([System.Drawing.Color]::FromArgb(0, 96, 165, 250))
    $g.FillPath($pb, $pg)
}

# logo
$logo = [System.Drawing.Image]::FromFile("$root\assets\logo.png")
$g.DrawImage($logo, 84, 96, 150, 150); $logo.Dispose()

# wordmark + tagline
$fTitle = New-Object System.Drawing.Font 'Segoe UI Semibold', 78, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$fTag = New-Object System.Drawing.Font 'Segoe UI', 30, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$g.DrawString('SnapDrop', $fTitle, [System.Drawing.Brushes]::White, 254, 76)
$g.DrawString('Screenshots you can drag anywhere.', $fTag, (New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(190, 205, 235))), 262, 196)

# pills
$fPill = New-Object System.Drawing.Font 'Segoe UI Semibold', 20, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$x = 262
foreach ($t in @('Ctrl + Shift + S', 'Drag it anywhere', 'Crop & mark up')) {
    $sz = $g.MeasureString($t, $fPill); $pw = [int]$sz.Width + 34; $ph = 42; $y = 262
    $gp = New-Object System.Drawing.Drawing2D.GraphicsPath
    $gp.AddArc($x, $y, $ph, $ph, 90, 180); $gp.AddArc($x + $pw - $ph, $y, $ph, $ph, 270, 180); $gp.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(46, 255, 255, 255))), $gp)
    $g.DrawPath((New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(70, 255, 255, 255)), 1), $gp)
    $g.DrawString($t, $fPill, [System.Drawing.Brushes]::White, $x + 17, $y + 8)
    $x += $pw + 14
}

# right side: the real thumbnail screenshot as a floating card
$thumbPath = "$root\docs\img\thumbnail.png"
if (Test-Path $thumbPath) {
    $t = [System.Drawing.Image]::FromFile($thumbPath)
    $cw = 320; $ch = [int]($t.Height * $cw / $t.Width); $cx = $CanvasW - $cw - 70; $cy = ($CanvasH - $ch) / 2 + 6
    for ($i = 14; $i -ge 1; $i--) {   # soft shadow
        $sg = New-Object System.Drawing.Drawing2D.GraphicsPath
        $r = 26 + $i; $sx = $cx - $i; $sy = $cy - $i + 12; $sw = $cw + 2 * $i; $sh = $ch + 2 * $i
        $sg.AddArc($sx, $sy, $r, $r, 180, 90); $sg.AddArc($sx + $sw - $r, $sy, $r, $r, 270, 90); $sg.AddArc($sx + $sw - $r, $sy + $sh - $r, $r, $r, 0, 90); $sg.AddArc($sx, $sy + $sh - $r, $r, $r, 90, 90); $sg.CloseFigure()
        $g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(7, 0, 0, 0))), $sg)
    }
    $clip = New-Object System.Drawing.Drawing2D.GraphicsPath
    $r = 28
    $clip.AddArc($cx, $cy, $r, $r, 180, 90); $clip.AddArc($cx + $cw - $r, $cy, $r, $r, 270, 90); $clip.AddArc($cx + $cw - $r, $cy + $ch - $r, $r, $r, 0, 90); $clip.AddArc($cx, $cy + $ch - $r, $r, $r, 90, 90); $clip.CloseFigure()
    $g.SetClip($clip); $g.DrawImage($t, $cx, $cy, $cw, $ch); $g.ResetClip()
    $g.DrawPath((New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(90, 255, 255, 255)), 2), $clip)
    $t.Dispose()
}

$g.Dispose()
$bmp.Save("$root\assets\banner.png", [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
"Wrote $root\assets\banner.png"
