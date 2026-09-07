#!/bin/bash
# ========================================
# MIXANDMIN v3.0 - Build Script (Linux/Mac)
# ========================================

echo ""
echo "╔════════════════════════════════════════╗"
echo "║   MIXANDMIN v3.0 - Build Script        ║"
echo "║   Production Ready Build                ║"
echo "╚════════════════════════════════════════╝"
echo ""

# Check if dotnet is installed
echo "[*] Checking for .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "[ERROR] .NET SDK not found! Please install .NET 10.0"
    exit 1
fi
echo "[OK] .NET SDK found"

# Change to project directory
cd "$(dirname "$0")/decompiled"
echo "[*] Working directory: $(pwd)"

# Clean previous build
echo ""
echo "[*] Cleaning previous build..."
dotnet clean LOVESIXHub.csproj -c Release 2>/dev/null
rm -rf bin/Release 2>/dev/null
echo "[OK] Clean complete"

# Restore dependencies
echo ""
echo "[*] Restoring dependencies..."
dotnet restore LOVESIXHub.csproj
if [ $? -ne 0 ]; then
    echo "[ERROR] Failed to restore dependencies"
    exit 1
fi
echo "[OK] Dependencies restored"

# Build project
echo ""
echo "[*] Building MIXANDMIN v3.0..."
echo "[*] Framework: net10.0-windows"
echo "[*] Configuration: Release"
echo ""
dotnet build LOVESIXHub.csproj -c Release --no-restore
if [ $? -ne 0 ]; then
    echo ""
    echo "[ERROR] Build failed!"
    exit 1
fi
echo "[OK] Build successful"

# Publish
echo ""
echo "[*] Publishing application..."
dotnet publish LOVESIXHub.csproj -c Release -o ./publish --no-build
if [ $? -ne 0 ]; then
    echo "[ERROR] Publish failed"
    exit 1
fi
echo "[OK] Publish complete"

# Create output directory
echo ""
echo "[*] Creating distribution package..."
mkdir -p ../dist

# Copy executable and dependencies
echo "[*] Copying files...
cp -r publish/* ../dist/ 2>/dev/null

# Copy WinDivert files if they exist
[ -f "WinDivert.dll" ] && cp WinDivert.dll ../dist/
[ -f "WinDivert64.sys" ] && cp WinDivert64.sys ../dist/

echo "[OK] Files copied"

# Create launcher script
echo ""
echo "[*] Creating launcher...
cat > ../dist/run.sh << 'EOF'
#!/bin/bash
cd "$(dirname "$0")"
./MIXANDMIN &
EOF
chmod +x ../dist/run.sh

echo "[OK] Launcher created"

# Display result
echo ""
echo "╔════════════════════════════════════════╗"
echo "║          BUILD COMPLETE ✓              ║"
echo "╚════════════════════════════════════════╝"
echo ""
echo "[INFO] Output location: ../dist/"
echo "[INFO] Executable: MIXANDMIN"
echo "[INFO] Launcher: run.sh"
echo ""
echo "[*] Files included:"
echo "    - MIXANDMIN (Main application)"
echo "    - All .NET dependencies"
echo "    - WinDivert.dll (Network driver)"
echo "    - WinDivert64.sys (System driver)"
echo "    - run.sh (Quick launcher)"
echo ""
echo "[*] Ready to use! Run: ./run.sh"
echo ""
