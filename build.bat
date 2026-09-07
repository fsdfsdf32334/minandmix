@echo off
REM ========================================
REM MIXANDMIN v3.0 - Build Script
REM ========================================

setlocal enabledelayedexpansion

echo.
echo ╔════════════════════════════════════════╗
echo ║   MIXANDMIN v3.0 - Build Script        ║
echo ║   Production Ready Build                ║
echo ╚════════════════════════════════════════╝
echo.

REM Check if dotnet is installed
echo [*] Checking for .NET SDK...
where dotnet >nul 2>nul
if !errorlevel! neq 0 (
    echo [ERROR] .NET SDK not found! Please install .NET 10.0
    pause
    exit /b 1
)
echo [OK] .NET SDK found

REM Change to project directory
cd /d "%~dp0decompiled"
echo [*] Working directory: %cd%

REM Clean previous build
echo.
echo [*] Cleaning previous build...
dotnet clean LOVESIXHub.csproj -c Release 2>nul
if exist "bin\Release" rmdir /s /q "bin\Release" 2>nul
echo [OK] Clean complete

REM Restore dependencies
echo.
echo [*] Restoring dependencies...
dotnet restore LOVESIXHub.csproj
if !errorlevel! neq 0 (
    echo [ERROR] Failed to restore dependencies
    pause
    exit /b 1
)
echo [OK] Dependencies restored

REM Build project
echo.
echo [*] Building MIXANDMIN v3.0...
echo [*] Framework: net10.0-windows
echo [*] Configuration: Release
echo.
dotnet build LOVESIXHub.csproj -c Release --no-restore
if !errorlevel! neq 0 (
    echo.
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [OK] Build successful

REM Publish
echo.
echo [*] Publishing application...
dotnet publish LOVESIXHub.csproj -c Release -o "./publish" --no-build
if !errorlevel! neq 0 (
    echo [ERROR] Publish failed
    pause
    exit /b 1
)
echo [OK] Publish complete

REM Create output directory
echo.
echo [*] Creating distribution package...
if not exist "..\dist" mkdir "..\dist"

REM Copy executable and dependencies
echo [*] Copying files...
xcopy /E /I /Y "publish\*.*" "..\dist\" >nul 2>&1

REM Verify WinDivert files
if exist "WinDivert.dll" copy /Y "WinDivert.dll" "..\dist\" >nul
if exist "WinDivert64.sys" copy /Y "WinDivert64.sys" "..\dist\" >nul

echo [OK] Files copied

REM Create batch launcher
echo.
echo [*] Creating launcher...
(
    echo @echo off
    echo cd /d "%%~dp0"
    echo start MIXANDMIN.exe
) > "..\dist\run.bat"

echo [OK] Launcher created

REM Display result
echo.
echo ╔════════════════════════════════════════╗
echo ║          BUILD COMPLETE ✓              ║
echo ╚════════════════════════════════════════╝
echo.
echo [INFO] Output location: %~dp0.\dist\
echo [INFO] Executable: MIXANDMIN.exe
echo [INFO] Launcher: run.bat
echo.
echo [*] Files included:
echo     - MIXANDMIN.exe (Main application)
echo     - All .NET dependencies
echo     - WinDivert.dll (Network driver)
echo     - WinDivert64.sys (System driver)
echo     - run.bat (Quick launcher)
echo.
echo [*] Ready to use! You can:
echo     1. Run "run.bat" to start the app
echo     2. Or double-click "MIXANDMIN.exe"
echo.
echo [TIP] Create shortcut to MIXANDMIN.exe for quick access
echo.
pause
exit /b 0
