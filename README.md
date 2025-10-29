# Mongolian Phonetic Keyboard for Windows

A native Windows application that provides phonetic Mongolian keyboard layout without requiring AutoHotKey or Keyman.

**Author:** Amgaa G.
**License:** LGPL v3

## Features

- ✅ **Native Windows Application** - No AutoHotKey or Keyman required
- ✅ **System Tray Integration** - Easy enable/disable toggle
- ✅ **Auto-start Option** - Automatically start with Windows
- ✅ **Full Phonetic Support** - Complete Cyrillic mapping with multi-character combinations
- ✅ **Lightweight** - Single executable, minimal resource usage

## Keyboard Mappings

### Single Character Mappings

```
a→а  b→б  v→в  g→г  d→д  e→э  j→ж  z→з  i→и  k→к
l→л  m→м  n→н  o→о  p→п  r→р  s→с  t→т  u→у  f→ф
h→х  x→х  c→ц  y→ы  q→ө  w→ү  '→ь  "→ъ
```

### Multi-Character Combinations

```
ye → е     yo → ё     ts → ц     ch → ч     sh → ш
yu → ю     ya → я     ai → ай    ei → эй    oi → ой
ui → уй    qi → өй    wi → үй    ii → ий
```

### Special Characters

```
'' (double apostrophe) → Ь
"" (double quote) → Ъ
```

### Examples

```
Type: sain baina uu    → Output: сайн байна уу
Type: mongol           → Output: монгол
Type: bayarlalaa       → Output: баярлалаа
Type: orchuulga        → Output: орчуулга
```

## Installation

### Option 1: Use Pre-built Installer (Recommended)

1. Download `MongolianPhoneticKeyboard-Setup-1.0.0.exe` from releases
2. Run the installer
3. Choose whether to start with Windows
4. Click Install

### Option 2: Build from Source

#### Prerequisites

- Windows 10 or later
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

#### Build Steps

**On Windows (PowerShell):**

```powershell
.\build.ps1
```

**On Linux/WSL:**

```bash
chmod +x build.sh
./build.sh
```

The executable will be created in the `publish/` directory.

#### Create Installer (Optional)

1. Download and install [Inno Setup](https://jrsoftware.org/isdl.php)
2. Build the executable first using the build script
3. Right-click `installer.iss` and select "Compile"
4. The installer will be created in the `installer/` directory

## Usage

### Running the Application

1. Double-click `MongolianPhonetic.exe` or install via the installer
2. Look for the **М** icon in the system tray
3. The keyboard is **enabled by default**

### System Tray Controls

Right-click the tray icon for options:

- **Enabled** - Toggle keyboard on/off (or double-click the icon)
- **Start with Windows** - Enable/disable auto-start
- **About** - View application information
- **Exit** - Close the application

### Keyboard Shortcuts

- `Ctrl+[key]` - Shortcuts are passed through (not intercepted)
- `Alt+[key]` - Shortcuts are passed through (not intercepted)

### Testing the Keyboard

Open Notepad or any text editor and try typing:

```
sain baina uu
```

You should see:

```
сайн байна уу
```

## Technical Details

### How It Works

The application uses Windows low-level keyboard hooks to intercept keystrokes and perform real-time character replacements. It maintains a state machine to handle multi-character combinations.

**Components:**

- **KeyboardHook.cs** - Low-level keyboard hook using Windows API
- **MongolianPhoneticMapper.cs** - Mapping logic and state machine
- **TrayApplication.cs** - System tray interface and auto-start configuration
- **Program.cs** - Application entry point

### Architecture

```
User types 'y' → KeyboardHook intercepts
                → MongolianPhoneticMapper processes
                → Outputs 'ы'
                → Remembers last character

User types 'e' → KeyboardHook intercepts
                → MongolianPhoneticMapper checks: 'ы' + 'e'?
                → Combination found: 'е'
                → Sends backspace to remove 'ы'
                → Outputs 'е'
```

### Security Considerations

- The application requires low-level keyboard hook permissions
- It only intercepts letter keys and passes through all shortcuts
- No data is logged or transmitted
- Source code is open and auditable

## Troubleshooting

### Application Won't Start

- Ensure .NET 6.0 runtime is installed
- Check if another instance is already running
- Run as Administrator if needed

### Keys Not Being Mapped

- Verify the application is running (check system tray)
- Make sure it's enabled (double-click tray icon to toggle)
- Ensure you're not using Ctrl or Alt (shortcuts are passed through)

### Auto-Start Not Working

- Check Windows Task Manager → Startup tab
- Verify registry entry: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- Try disabling and re-enabling auto-start

### Conflicts with Other Keyboard Software

If you have AutoHotKey, Keyman, or other keyboard software running:

1. Exit or disable the other software
2. Restart Mongolian Phonetic Keyboard

## Building Installer

To create a professional installer:

1. Build the application:
   ```powershell
   .\build.ps1
   ```

2. Install [Inno Setup](https://jrsoftware.org/isdl.php)

3. (Optional) Create an icon file named `icon.ico` in the project root

4. Compile the installer:
   ```
   Right-click installer.iss → Compile
   ```

5. The installer will be in `installer/MongolianPhoneticKeyboard-Setup-1.0.0.exe`

## Development

### Project Structure

```
MongolianPhonetic-Windows/
├── MongolianPhonetic.csproj    # Project file
├── Program.cs                   # Entry point
├── KeyboardHook.cs              # Low-level keyboard hook
├── MongolianPhoneticMapper.cs   # Mapping logic
├── TrayApplication.cs           # System tray UI
├── build.ps1                    # Build script (Windows)
├── build.sh                     # Build script (Linux/WSL)
├── installer.iss                # Inno Setup installer script
├── LICENSE.txt                  # LGPL v3 license
└── README.md                    # This file
```

### Modifying Mappings

To add or modify character mappings, edit `MongolianPhoneticMapper.cs`:

1. **Single character mappings**: Update `_lowerCaseMap` or `_upperCaseMap` dictionaries
2. **Multi-character combinations**: Update the `CheckCombination` method

After modifications, rebuild:

```powershell
.\build.ps1
```

## Related Projects

- **Linux version**: IBus table format (see `../mapping.txt`)
- **Keyman version**: See `../mongolian-phonetic.kmn`

## License

This project is licensed under the GNU Lesser General Public License v3.0.

See [LICENSE.txt](LICENSE.txt) for details.

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## Credits

Based on the Mongolian phonetic keyboard layout originally developed for Linux IBus.

## Support

For issues or questions:

- Create an issue on GitHub
- Contact: Amgaa G.

---

**Note:** This application works at the system level by hooking keyboard input. Some antivirus software may flag keyboard hooks as suspicious. This is a false positive - the application is open source and does not contain malware.
