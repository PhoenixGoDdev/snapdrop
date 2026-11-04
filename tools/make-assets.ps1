# Generates the app icon (assets/icon.ico) and the installer artwork (installer/*.bmp).
# Run from anywhere:  powershell -ExecutionPolicy Bypass -File tools\make-assets.ps1
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
New-Item -ItemType Directory -Force "$root\assets", "$root\installer" | Out-Null

function New-Logo([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.PixelOffsetMode = 'HighQuality'; $g.Clear([System.Drawing.Color]::Transparent)
    $s = [single]$size; $pad = $s * 0.04; $r = $s * 0.24
    $gp = New-Object System.Drawing.Drawing2D.GraphicsPath
    $x = $pad; $y = $pad; $w = $s - 2 * $pad
    $gp.AddArc($x, $y, 2 * $r, 2 * $r, 180, 90); $gp.AddArc($x + $w - 2 * $r, $y, 2 * $r, 2 * $r, 270, 90)
    $gp.AddArc($x + $w - 2 * $r, $y + $w - 2 * $r, 2 * $r, 2 * $r, 0, 90); $gp.AddArc($x, $y + $w - 2 * $r, 2 * $r, 2 * $r, 90, 90); $gp.CloseFigure()
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush ((New-Object System.Drawing.PointF 0, 0), (New-Object System.Drawing.PointF $s, $s), [System.Drawing.Color]::FromArgb(96, 165, 250), [System.Drawing.Color]::FromArgb(37, 99, 235))
    $g.FillPath($grad, $gp)
    # viewfinder corners
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), ([single]($s * 0.075))
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'; $pen.LineJoin = 'Round'
    $a = $s * 0.25; $b = $s * 0.75; $l = $s * 0.17
    foreach ($c in @(@($a, $a, 1, 1), @($b, $a, -1, 1), @($a, $b, 1, -1), @($b, $b, -1, -1))) {
        $px = [single]$c[0]; $py = [single]$c[1]; $dx = [single]$c[2]; $dy = [single]$c[3]
        $pts = [System.Drawing.PointF[]]@((New-Object System.Drawing.PointF ($px), ($py + $dy * $l)), (New-Object System.Drawing.PointF $px, $py), (New-Object System.Drawing.PointF ($px + $dx * $l), $py))
        $g.DrawLines($pen, $pts)
    }
    $d = $s * 0.17
    $g.FillEllipse([System.Drawing.Brushes]::White, ($s - $d) / 2, ($s - $d) / 2, $d, $d)
    $g.Dispose()
    return $bmp
}

# ---- icon.ico (PNG-compressed entries) ----
$sizes = 16, 24, 32, 48, 64, 128, 256
$pngs = foreach ($sz in $sizes) { $b = New-Logo $sz; $ms = New-Object System.IO.MemoryStream; $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); $b.Dispose(); , $ms.ToArray() }
$ico = New-Object System.IO.MemoryStream; $bw = New-Object System.IO.BinaryWriter $ico
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $sz = $sizes[$i]; $wb = if ($sz -ge 256) { 0 } else { $sz }
    $bw.Write([byte]$wb); $bw.Write([byte]$wb); $bw.Write([byte]0); $bw.Write([byte]0); $bw.Write([uint16]1); $bw.Write([uint16]32)
    $bw.Write([uint32]$pngs[$i].Length); $bw.Write([uint32]$offset); $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $bw.Write($p) }
[IO.File]::WriteAllBytes("$root\assets\icon.ico", $ico.ToArray())
$big = New-Logo 512; $big.Save("$root\assets\logo.png", [System.Drawing.Imaging.ImageFormat]::Png); $big.Dispose()

# ---- installer artwork ----
function New-Wizard([int]$w, [int]$h, [string]$path) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.TextRenderingHint = 'ClearTypeGridFit'; $g.InterpolationMode = 'HighQualityBicubic'
    $bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush ((New-Object System.Drawing.Rectangle 0, 0, $w, $h), [System.Drawing.Color]::FromArgb(18, 20, 28), [System.Drawing.Color]::FromArgb(23, 42, 92), 62.0)
    $g.FillRectangle($bg, 0, 0, $w, $h)
    $logoSize = [int]($w * 0.52)
    $logo = New-Logo ($logoSize * 2)
    $g.DrawImage($logo, [int](($w - $logoSize) / 2), [int]($h * 0.16), $logoSize, $logoSize); $logo.Dispose()
    $f1 = New-Object System.Drawing.Font 'Segoe UI Semibold', ([single]($w * 0.115)), ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    $f2 = New-Object System.Drawing.Font 'Segoe UI', ([single]($w * 0.062)), ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    $sf = New-Object System.Drawing.StringFormat; $sf.Alignment = 'Center'
    $ty = $h * 0.16 + $logoSize + $h * 0.05
    $g.DrawString('SnapDrop', $f1, [System.Drawing.Brushes]::White, (New-Object System.Drawing.RectangleF 0, $ty, $w, ($w * 0.2)), $sf)
    $g.DrawString("Screenshots you can`ndrag anywhere", $f2, (New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(170, 190, 225))), (New-Object System.Drawing.RectangleF 0, ($ty + $w * 0.17), $w, ($w * 0.3)), $sf)
    $g.Dispose(); $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Bmp); $bmp.Dispose()
}
New-Wizard 164 314 "$root\installer\wizard-large.bmp"
New-Wizard 246 471 "$root\installer\wizard-large@1.5x.bmp"
New-Wizard 328 628 "$root\installer\wizard-large@2x.bmp"

function New-Small([int]$n, [string]$path) {
    $bmp = New-Object System.Drawing.Bitmap $n, $n
    $g = [System.Drawing.Graphics]::FromImage($bmp); $g.Clear([System.Drawing.Color]::White); $g.InterpolationMode = 'HighQualityBicubic'
    $logo = New-Logo ($n * 2); $g.DrawImage($logo, 2, 2, $n - 4, $n - 4); $logo.Dispose(); $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Bmp); $bmp.Dispose()
}
New-Small 55 "$root\installer\wizard-small.bmp"
New-Small 83 "$root\installer\wizard-small@1.5x.bmp"
New-Small 110 "$root\installer\wizard-small@2x.bmp"
"Assets written to $root\assets and $root\installer"
