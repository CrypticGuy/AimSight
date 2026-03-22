# Generate-Icon.ps1 - Creates tray-icon.png matching the programmatic tray icon
Add-Type -AssemblyName System.Drawing

$size = 64  # Higher res for display
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)

$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 255, 0, 0), [int]($size / 16))
$pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round

# Cross lines
$center = $size / 2
$graphics.DrawLine($pen, $center, 2, $center, $size - 2)
$graphics.DrawLine($pen, 2, $center, $size - 2, $center)

# Circle
$circleSize = $size * 11 / 16
$circleOffset = ($size - $circleSize) / 2
$graphics.DrawEllipse($pen, $circleOffset, $circleOffset, $circleSize, $circleSize)

$outputPath = "$PSScriptRoot\tray-icon.png"
$bitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)

$pen.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Icon generated: $outputPath"
