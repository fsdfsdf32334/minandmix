# Quick Setup Guide

## Step 1: Build

### Windows
```batch
cd [project-directory]
build.bat
```

### Linux/Mac
```bash
cd [project-directory]
chmod +x build.sh
./build.sh
```

## Step 2: Locate Output

Find the built application in:
```
./dist/MIXANDMIN.exe (Windows)
./dist/MIXANDMIN (Linux/Mac)
```

## Step 3: Run

### Method 1: Batch/Shell Script
```batch
# Windows
.\dist\run.bat

# Linux/Mac
./dist/run.sh
```

### Method 2: Direct Execution
```batch
# Windows
.\dist\MIXANDMIN.exe

# Linux/Mac
./dist/MIXANDMIN
```

## Step 4: First Launch

1. Application initializes
2. Creates config folder: `%APPDATA%\MIXANDMIN\`
3. Sets up default settings
4. Displays main window

## Configuration

Edit: `%APPDATA%\MIXANDMIN\config.json`

## Logs

View: `%APPDATA%\MIXANDMIN\logs\`

## Features Ready to Use

- ✅ Advanced Statistics Dashboard
- ✅ Performance Monitoring (CPU/RAM)
- ✅ Network Diagnostics
- ✅ Macro Recording & Playback
- ✅ Backup & Recovery
- ✅ Multi-Theme Support
- ✅ Hotkey Profiles
- ✅ Complete Logging System
- ✅ Health Checks
- ✅ And more...

## Troubleshooting

### Application won't start
1. Check logs in `%APPDATA%\MIXANDMIN\logs\`
2. Ensure .NET 10.0 is installed: `dotnet --version`
3. Run as Administrator

### Missing WinDivert
- Ensure `WinDivert.dll` and `WinDivert64.sys` are in dist folder
- Run as Administrator
- Check Windows Defender quarantine

## Need Help?

Check these files:
- `BUILD_GUIDE.md` - Detailed build instructions
- `CHANGELOG.md` - Version history & features
- `decompiled/LOVESIX/` - Source code

---

**You're all set! Enjoy MIXANDMIN v3.0 🎉**
