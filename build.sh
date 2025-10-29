#!/bin/bash
# Build script for Mongolian Phonetic Keyboard (Linux/WSL)
# This script builds a self-contained executable for Windows

echo "Building Mongolian Phonetic Keyboard..."

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed!"
    echo "Please download and install .NET 6.0 SDK or later from:"
    echo "https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo ".NET SDK version: $DOTNET_VERSION"

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf bin obj publish

# Build the application
echo "Building application..."
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish

if [ $? -eq 0 ]; then
    echo ""
    echo "Build successful!"
    echo "Executable location: publish/MongolianPhonetic.exe"
    echo ""
    echo "You can now:"
    echo "1. Copy the executable to Windows and run it"
    echo "2. Create an installer using Inno Setup with installer.iss"
else
    echo ""
    echo "Build failed!"
    exit 1
fi
