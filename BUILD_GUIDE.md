# MIXANDMIN v3.0 Build Guide

## Quick Start

### Windows
```batch
build.bat
```

### Linux/Mac
```bash
chmod +x build.sh
./build.sh
```

## Build Output

After successful build, find your application in:
```
./dist/
├── MIXANDMIN.exe (Windows) or MIXANDMIN (Linux/Mac)
├── run.bat (Windows) or run.sh (Linux/Mac)
├── WinDivert.dll
├── WinDivert64.sys
└── [all .NET dependencies]
```

## Requirements

- **.NET SDK 10.0** or higher
- **Windows 10/11** or **Linux/Mac with .NET support**
- **Admin privileges** (for WinDivert driver installation)

## Installation & Deployment

### Option 1: Direct Execution
```bash
# Windows
.\dist\run.bat

# Linux/Mac
./dist/run.sh
```

### Option 2: Create Shortcut
Right-click `MIXANDMIN.exe` → Send to → Desktop (create shortcut)

### Option 3: System Installation
```batch
# Run as Administrator
cd dist
MIXANDMIN.exe
```

## First Launch

On first run, the application will:
1. Initialize configuration files
2. Create log directories
3. Load default settings
4. Perform system health check
5. Display welcome screen

## Configuration

After first run, find config at:
```
%APPDATA%\MIXANDMIN\config.json
```

Edit this file to customize:
- Theme (Dark/Light/Auto)
- Auto-start behavior
- Feature toggles
- Update intervals

## Troubleshooting

### Build Fails
```bash
# Ensure .NET is installed
dotnet --version

# Clear NuGet cache
dotnet nuget locals all --clear

# Try building again
```

### Runtime Errors

Check logs at:
```
%APPDATA%\MIXANDMIN\logs\
```

### WinDivert Issues

1. Run as Administrator
2. Ensure WinDivert.dll and WinDivert64.sys are in same directory as MIXANDMIN.exe
3. Check Windows Defender hasn't quarantined WinDivert

## System Requirements

### Minimum
- **OS:** Windows 10 / Linux / macOS
- **RAM:** 256 MB
- **CPU:** 2 GHz dual-core
- **.NET:** 10.0 Runtime

### Recommended
- **OS:** Windows 11
- **RAM:** 512 MB+
- **CPU:** 4-core 2.5 GHz+
- **.NET:** 10.0 SDK (for development)

## Features Summary

✅ **25+ Advanced Features**
- Multi-theme support (Dark/Light/Auto)
- Real-time performance monitoring
- Network diagnostics
- Cheat history & undo/redo
- Macro recording & playback
- Backup & recovery system
- Advanced statistics dashboard
- Hotkey profile management
- Comprehensive logging
- Health check system

## Support & Documentation

For issues or questions:
1. Check logs in `%APPDATA%\MIXANDMIN\logs\`
2. Review configuration in `%APPDATA%\MIXANDMIN\config.json`
3. Check GitHub issues
4. Review CHANGELOG.md for version info

## Version Info

- **Current Version:** 3.0.0
- **Build Date:** 2026-09-07
- **Status:** Production Ready ✅
- **License:** Proprietary

---

**Ready to use! Enjoy MIXANDMIN v3.0! 🚀**
