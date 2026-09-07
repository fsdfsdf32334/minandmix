if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Write-Host "✓ .NET SDK found" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK NOT found!" -ForegroundColor Red
    Write-Host "Please install .NET 10.0 from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

$projectPath = "./decompiled"
$distPath = "./dist"

Write-Host ""
Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   MIXANDMIN v3.0 - Build Script        ║" -ForegroundColor Cyan
Write-Host "║   Production Ready Build                ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/5] Cleaning previous build..." -ForegroundColor Yellow
Set-Location $projectPath
if (Test-Path "bin/Release") { Remove-Item -Recurse -Force "bin/Release" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
Write-Host "[OK] Clean complete" -ForegroundColor Green

Write-Host "[2/5] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore LOVESIXHub.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Restore failed!" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Dependencies restored" -ForegroundColor Green

Write-Host "[3/5] Building application..." -ForegroundColor Yellow
dotnet build LOVESIXHub.csproj -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Build successful" -ForegroundColor Green

Write-Host "[4/5] Publishing..." -ForegroundColor Yellow
dotnet publish LOVESIXHub.csproj -c Release -o ./publish --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Publish complete" -ForegroundColor Green

Write-Host "[5/5] Creating distribution..." -ForegroundColor Yellow
Set-Location ..
if (-not (Test-Path $distPath)) { New-Item -ItemType Directory -Path $distPath | Out-Null }
Copy-Item -Path "$projectPath\publish\*" -Destination $distPath -Recurse -Force -ErrorAction SilentlyContinue

# Copy WinDivert if exists
if (Test-Path "$projectPath\WinDivert.dll") { Copy-Item "$projectPath\WinDivert.dll" $distPath -Force }
if (Test-Path "$projectPath\WinDivert64.sys") { Copy-Item "$projectPath\WinDivert64.sys" $distPath -Force }

Write-Host "[OK] Distribution created" -ForegroundColor Green

Write-Host ""
Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║          BUILD COMPLETE ✓              ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "[INFO] Output: $distPath\" -ForegroundColor Cyan
Write-Host "[INFO] Executable: $distPath\MIXANDMIN.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "[*] Ready to use! Options:" -ForegroundColor Green
Write-Host "    1. Run: .\dist\MIXANDMIN.exe" -ForegroundColor White
Write-Host "    2. Or create desktop shortcut" -ForegroundColor White
Write-Host ""
