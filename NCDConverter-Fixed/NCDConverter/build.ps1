# Build Script for NCD Converter (PowerShell)
# Can be run on Windows, macOS, or Linux

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "NCD to Parquet Converter Builder" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: .NET SDK not found" -ForegroundColor Red
    Write-Host "Please install .NET 6.0 SDK or later from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Clean previous builds
Write-Host ""
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "NCDConverter/bin") { Remove-Item "NCDConverter/bin" -Recurse -Force }
if (Test-Path "NCDConverter/obj") { Remove-Item "NCDConverter/obj" -Recurse -Force }

# Restore packages
Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore NCDConverter.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Failed to restore packages" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Build Release
Write-Host "[3/4] Building Release version..." -ForegroundColor Yellow
dotnet build NCDConverter.sln -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Build failed" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Publish
Write-Host "[4/4] Publishing application..." -ForegroundColor Yellow
dotnet publish NCDConverter/NCDConverter.csproj -c Release -r win-x64 --self-contained false -o "./publish"
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Publish failed" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host "✓ Build completed successfully!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output location: ./publish/" -ForegroundColor Cyan
Write-Host "Executable: ./publish/NCDConverter.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Yellow
Write-Host "  ./publish/NCDConverter.exe" -ForegroundColor White
Write-Host ""
Read-Host "Press Enter to exit"
