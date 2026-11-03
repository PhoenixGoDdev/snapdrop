# Builds a single-file SnapDrop.exe into .\dist
Set-Location $PSScriptRoot
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o dist
if ($LASTEXITCODE -eq 0) { Write-Host "`nBuilt: $PSScriptRoot\dist\SnapDrop.exe" -ForegroundColor Green }
