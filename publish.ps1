# Build MobaCompanion for distribution (one compressed exe + zip, target < 100 MB).
# Run: powershell -ExecutionPolicy Bypass -File publish.ps1

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$outSingle = Join-Path $root "dist\single"
$zip = Join-Path $root "dist\MobaCompanion-portable.zip"

Remove-Item (Join-Path $root "dist") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $root "bin") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $root "obj") -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $outSingle -Force | Out-Null

Write-Host "Publishing compressed single-file win-x64..."

dotnet publish (Join-Path $root "MobaCompanion.csproj") -c Release `
  -o $outSingle `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:DebugType=none `
  -p:DebugSymbols=false

# Убрать отладочные/лишние файлы рядом с exe
Get-ChildItem $outSingle -File | Where-Object {
  $_.Extension -in ".pdb", ".xml", ".deps.json" -or $_.Name -like "*.pdb"
} | Remove-Item -Force -ErrorAction SilentlyContinue

$exe = Join-Path $outSingle "MobaCompanion.exe"
if (-not (Test-Path $exe)) { throw "MobaCompanion.exe not found after publish" }

$sizeMb = [math]::Round((Get-Item $exe).Length / 1MB, 1)
Write-Host "Single exe size: $sizeMb MB"

if ($sizeMb -gt 100) {
  Write-Warning "Exe exceeds 100 MB ($sizeMb). Check dependencies."
}

Compress-Archive -Path $exe -DestinationPath $zip -Force

# Сборочный мусор не оставляем
Remove-Item (Join-Path $root "bin") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $root "obj") -Recurse -Force -ErrorAction SilentlyContinue

$zipMb = [math]::Round((Get-Item $zip).Length / 1MB, 1)
Write-Host ""
Write-Host "Done:"
Write-Host "  Single exe:  $exe  ($sizeMb MB)"
Write-Host "  ZIP:         $zip  ($zipMb MB)"
