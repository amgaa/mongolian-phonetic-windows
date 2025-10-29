# Build script for Mongolian Phonetic Keyboard
# This script builds a self-contained executable for Windows

Write-Host "Building Mongolian Phonetic Keyboard..." -ForegroundColor Green

# Check if .NET SDK is installed
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Host "Error: .NET SDK is not installed!" -ForegroundColor Red
    Write-Host "Please download and install .NET 6.0 SDK or later from:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ".NET SDK version: $dotnetVersion" -ForegroundColor Cyan

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin"
}
if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj"
}
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}

# Build the application
Write-Host "Building application..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Executable location: publish\MongolianPhonetic.exe" -ForegroundColor Cyan
    Write-Host "`nYou can now:" -ForegroundColor Yellow
    Write-Host "1. Run the executable directly: .\publish\MongolianPhonetic.exe" -ForegroundColor White
    Write-Host "2. Create an installer using Inno Setup with installer.iss" -ForegroundColor White
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}
