# Quick Start Guide

## For End Users

### Installation

1. Download `MongolianPhoneticKeyboard-Setup-1.0.0.exe`
2. Run the installer
3. Check "Start with Windows" if you want it to auto-start
4. Click Install

### Usage

1. Look for the **М** icon in your system tray (bottom-right corner)
2. The keyboard is enabled by default
3. Open any text editor (Notepad, Word, etc.)
4. Start typing phonetically!

**Example:**
- Type: `sain baina uu`
- Output: `сайн байна уу`

### Toggle On/Off

- **Double-click** the tray icon to enable/disable
- Or **right-click** → select "Enabled"

### Uninstall

- Control Panel → Programs → Uninstall "Mongolian Phonetic Keyboard"
- Or run the uninstaller from Start Menu

---

## For Developers

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Build in 3 Steps

#### 1. Clone or download the source

```bash
cd MongolianPhonetic-Windows
```

#### 2. Build the executable

**Windows:**
```powershell
.\build.ps1
```

**Linux/WSL:**
```bash
chmod +x build.sh
./build.sh
```

#### 3. Run it

```bash
.\publish\MongolianPhonetic.exe
```

### Create Installer (Optional)

1. Install [Inno Setup](https://jrsoftware.org/isdl.php)
2. Build the executable first (step 2 above)
3. Right-click `installer.iss` → "Compile"
4. Find installer in `installer/` directory

---

## Keyboard Reference Card

### Common Characters

| Type | Output | Type | Output |
|------|--------|------|--------|
| a    | а      | A    | А      |
| e    | э      | E    | Э      |
| i    | и      | I    | И      |
| o    | о      | O    | О      |
| u    | у      | U    | У      |
| q    | ө      | Q    | Ө      |
| w    | ү      | W    | Ү      |
| y    | ы      | Y    | Ы      |

### Multi-Character Combinations

| Type | Output | Type | Output |
|------|--------|------|--------|
| ye   | е      | yo   | ё      |
| ch   | ч      | sh   | ш      |
| ts   | ц      | yu   | ю      |
| ya   | я      | ai   | ай     |
| ei   | эй     | oi   | ой     |
| ui   | уй     | qi   | өй     |
| wi   | үй     | ii   | ий     |

### Practice Sentences

```
sain baina uu        → сайн байна уу
ta mongol hel yarij chadah uu → та монгол хэл ярьж чадах уу
bayarlalaa          → баярлалаа
ta yaadag baina     → та яадаг байна
mongol uls          → монгол улс
```

---

## Troubleshooting

**Problem:** Keys not being mapped
- **Solution:** Double-click the tray icon to enable

**Problem:** Application not starting
- **Solution:** Install .NET 8.0 Runtime from [here](https://dotnet.microsoft.com/download/dotnet/8.0)

**Problem:** Want to temporarily disable
- **Solution:** Double-click the tray icon (or right-click → Enabled)

**Problem:** Conflicts with other keyboard software
- **Solution:** Exit other keyboard software (AutoHotKey, Keyman, etc.)

---

## More Information

See [README.md](README.md) for complete documentation.
