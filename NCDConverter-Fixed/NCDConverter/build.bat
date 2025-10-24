@echo off
REM Build Script for NCD Converter - CORRECTED
REM Requirements: .NET 6.0 SDK installed

echo ================================
echo NCD to Parquet Converter Builder
echo ================================
echo.

REM Check if dotnet is available
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found. Please install .NET 6.0 SDK or later.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/5] Cleaning previous builds...
if exist "NCDConverter\bin" rmdir /s /q "NCDConverter\bin"
if exist "NCDConverter\obj" rmdir /s /q "NCDConverter\obj"
dotnet clean NCDConverter\NCDConverter.csproj >nul 2>&1

echo [2/5] Restoring NuGet packages for project...
dotnet restore NCDConverter\NCDConverter.csproj
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    echo Try running: dotnet restore NCDConverter\NCDConverter.csproj --force-evaluate
    pause
    exit /b 1
)

echo [3/5] Building Release version...
dotnet build NCDConverter\NCDConverter.csproj -c Release --no-restore
if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo BUILD FAILED! 
    echo ========================================
    echo.
    echo Common fixes:
    echo 1. Make sure .NET 6.0 SDK is installed
    echo 2. Run: dotnet restore NCDConverter\NCDConverter.csproj --force-evaluate
    echo 3. Try opening in Visual Studio 2022
    echo 4. Check BUILD_FIXES.md for solutions
    echo.
    pause
    exit /b 1
)

echo [4/5] Publishing application...
dotnet publish NCDConverter\NCDConverter.csproj -c Release -r win-x64 --self-contained false -o ".\publish"
if %errorlevel% neq 0 (
    echo WARNING: Publish failed, but build succeeded
    echo You can run from: NCDConverter\bin\Release\net6.0-windows\NCDConverter.exe
    echo.
    echo Trying alternative publish...
    dotnet publish NCDConverter\NCDConverter.csproj -c Release -o ".\publish"
    if %errorlevel% neq 0 (
        echo Alternative publish also failed.
        echo Using build output instead...
        if not exist ".\publish" mkdir ".\publish"
        xcopy /Y /E "NCDConverter\bin\Release\net6.0-windows\*.*" ".\publish\"
    )
)

echo [5/5] Verifying output...
if exist ".\publish\NCDConverter.exe" (
    echo.
    echo ================================
    echo Build completed successfully!
    echo ================================
    echo.
    echo Executable: .\publish\NCDConverter.exe
    echo.
    echo To run: 
    echo   cd publish
    echo   NCDConverter.exe
    echo.
    echo Or double-click: publish\NCDConverter.exe
    echo.
) else if exist "NCDConverter\bin\Release\net6.0-windows\NCDConverter.exe" (
    echo.
    echo ================================
    echo Build completed!
    echo ================================
    echo.
    echo Executable location:
    echo   NCDConverter\bin\Release\net6.0-windows\NCDConverter.exe
    echo.
    echo To run: 
    echo   cd NCDConverter\bin\Release\net6.0-windows
    echo   NCDConverter.exe
    echo.
) else (
    echo.
    echo ERROR: Executable not found!
    echo Please check error messages above.
    echo.
)

pause
