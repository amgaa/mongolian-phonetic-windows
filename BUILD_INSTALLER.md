# How to Build the Installer

This guide will help you create a professional installer for the Mongolian Phonetic Keyboard.

## Prerequisites

1. **Build the application first**
   ```powershell
   .\build.ps1
   ```
   This creates `publish\MongolianPhonetic.exe`

2. **Download and install Inno Setup**
   - Download from: https://jrsoftware.org/isdl.php
   - Install the latest version (6.x or newer)

## Building the Installer

### Method 1: Using Inno Setup GUI (Recommended for first time)

1. Right-click on `installer.iss`
2. Select **"Compile"** (if Inno Setup is installed, this option appears)
3. Wait for compilation to complete
4. The installer will be created in: `installer\MongolianPhoneticKeyboard-Setup-1.0.0.exe`

### Method 2: Using Command Line

```powershell
# Compile the installer
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

## What the Installer Includes

- ✅ Single-file executable (self-contained, no .NET installation required)
- ✅ Optional auto-start with Windows
- ✅ Start menu shortcuts
- ✅ Clean uninstaller
- ✅ Automatic process termination if already running

## Installer Features

When users run the installer:

1. **Welcome screen** - Shows app name and version
2. **License agreement** - Shows LGPL license
3. **Installation directory** - Default: `C:\Program Files\Mongolian Phonetic Keyboard`
4. **Optional tasks:**
   - ☑️ Start with Windows (recommended)
5. **Installation progress**
6. **Completion:**
   - ☑️ Launch Mongolian Phonetic Keyboard (checked by default)

## Testing the Installer

1. **Build the installer** (see above)
2. **Run the installer:**
   ```
   .\installer\MongolianPhoneticKeyboard-Setup-1.0.0.exe
   ```
3. **Test installation:**
   - Choose "Start with Windows"
   - Click Install
   - App should launch automatically
4. **Test the app:**
   - Press `Ctrl+Shift+M` to toggle
   - Type some text
   - Check system tray icon
5. **Test uninstall:**
   - Control Panel → Programs → Uninstall
   - Or: Start Menu → Mongolian Phonetic Keyboard → Uninstall

## Distributing the Installer

The installer file is located at:
```
installer\MongolianPhoneticKeyboard-Setup-1.0.0.exe
```

This is a **single file** that contains everything needed. Users just need to:
1. Download the `.exe` file
2. Run it
3. Follow the installation wizard

**No .NET installation required** - the app is self-contained!

## Updating the Version

To create a new version:

1. **Update version in `installer.iss`:**
   ```
   #define MyAppVersion "1.1.0"
   ```

2. **Update version in `MongolianPhonetic.csproj`:**
   ```xml
   <Version>1.1.0</Version>
   ```

3. **Rebuild everything:**
   ```powershell
   .\build.ps1
   # Then compile installer
   ```

## Troubleshooting

### "File not found: publish\MongolianPhonetic.exe"
- Make sure you ran `.\build.ps1` first
- Check that `publish\MongolianPhonetic.exe` exists

### "Cannot find ISCC.exe"
- Install Inno Setup from https://jrsoftware.org/isdl.php
- Adjust the path in Method 2 if installed in a different location

### Antivirus warnings
- Some antivirus software may flag keyboard hooks as suspicious
- This is a false positive (the app is open source)
- You may need to sign the executable with a code signing certificate for production distribution

## Code Signing (Optional, for professional distribution)

To avoid antivirus warnings:

1. **Get a code signing certificate** (e.g., from DigiCert, Sectigo)
2. **Sign the executable:**
   ```powershell
   signtool sign /f "certificate.pfx" /p "password" /tr http://timestamp.digicert.com /td sha256 /fd sha256 "publish\MongolianPhonetic.exe"
   ```
3. **Rebuild the installer** - it will now contain the signed executable

## Creating a Portable Version

If you want a version that doesn't require installation:

1. **Just distribute `publish\MongolianPhonetic.exe`**
2. Users can run it directly from anywhere
3. No installer needed
4. User must manually configure auto-start if desired

The `.exe` is fully self-contained and portable!
