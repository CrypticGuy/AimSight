# Build-Package.ps1 - Build and package CrossHairPlus
# Usage: .\Build-Package.ps1 [-Configuration Release] [-OutputPath <path>]

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$OutputPath = "$PSScriptRoot\dist"
)

$ErrorActionPreference = "Stop"

$SolutionPath = "$PSScriptRoot\CrossHairPlus.sln"
$MainProjectPath = "$PSScriptRoot\CrossHairPlus\CrossHairPlus.csproj"
$OverlayHookProjectPath = "$PSScriptRoot\OverlayHook\OverlayHook.csproj"
$DistPath = $OutputPath

Write-Host "=== CrossHairPlus Build Script ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration"
Write-Host "Output Path: $DistPath"
Write-Host ""

# Clean output directory
if (Test-Path $DistPath) {
    Remove-Item $DistPath -Recurse -Force
}
New-Item -ItemType Directory -Path $DistPath | Out-Null

# Step 1: Build OverlayHook (NET Framework 4.8) - must be built first
Write-Host "[1/3] Building OverlayHook (NET Framework 4.8)..." -ForegroundColor Yellow
$overlayHookOutput = "$PSScriptRoot\OverlayHook\bin\$Configuration"
MSBuild $OverlayHookProjectPath `
    -p:Configuration=$Configuration `
    -p:Platform="x64" `
    -v:m

if ($LASTEXITCODE -ne 0) {
    Write-Error "OverlayHook build failed"
    exit 1
}
Write-Host "OverlayHook built successfully" -ForegroundColor Green

# Step 2: Restore and build main application
Write-Host "[2/3] Building AimSight (NET 9.0)..." -ForegroundColor Yellow
dotnet restore $SolutionPath
dotnet build $SolutionPath -c $Configuration -r win-x64

if ($LASTEXITCODE -ne 0) {
    Write-Error "Main application build failed"
    exit 1
}
Write-Host "AimSight built successfully" -ForegroundColor Green

# Step 3: Publish self-contained single-file executable
Write-Host "[3/3] Publishing self-contained executable..." -ForegroundColor Yellow
$publishPath = "$DistPath\publish"

dotnet publish $MainProjectPath `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed"
    exit 1
}

# Copy README to dist if it exists
$readmePath = "$PSScriptRoot\README.md"
if (Test-Path $readmePath) {
    Copy-Item $readmePath $DistPath
}

# Create version info
$version = "1.0.0"
$versionFile = "$DistPath\VERSION.txt"
@"
AimSight $version
Build: $Configuration
Built: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@ | Out-File $versionFile -Encoding UTF8

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Cyan
Write-Host "Output: $publishPath\AimSight.exe"
Write-Host ""
Get-ChildItem $publishPath -File | ForEach-Object {
    Write-Host "  $($_.Name) ($([math]::Round($_.Length / 1MB, 2)) MB)"
}
