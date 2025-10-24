@echo off
REM Installer Build Script
REM Requirements: WiX Toolset 3.11 or later

echo ================================
echo NCD Converter Installer Builder
echo ================================
echo.

REM Check if WiX is installed
where candle.exe >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: WiX Toolset not found
    echo Please install WiX Toolset 3.11 from:
    echo https://wixtoolset.org/releases/
    echo.
    echo After installation, add WiX bin folder to PATH:
    echo Example: C:\Program Files (x86)\WiX Toolset v3.11\bin
    pause
    exit /b 1
)

REM Ensure application is built first
if not exist "NCDConverter\bin\Release\net6.0-windows\NCDConverter.exe" (
    echo ERROR: Application not built yet
    echo Please run build.bat first
    pause
    exit /b 1
)

echo [1/3] Compiling WiX source...
cd Installer
candle.exe Installer.wxs -ext WixUIExtension
if %errorlevel% neq 0 (
    echo ERROR: WiX compilation failed
    cd ..
    pause
    exit /b 1
)

echo [2/3] Linking installer...
light.exe Installer.wixobj -ext WixUIExtension -out NCDConverterSetup.msi
if %errorlevel% neq 0 (
    echo ERROR: WiX linking failed
    cd ..
    pause
    exit /b 1
)

echo [3/3] Cleaning up temporary files...
del Installer.wixobj
del Installer.wixpdb

cd ..

echo.
echo ================================
echo Installer created successfully!
echo ================================
echo.
echo Output: Installer\NCDConverterSetup.msi
echo.
echo You can now distribute this MSI file to users.
echo.
pause
