# Quick Start Guide

## For Users (Installing & Running)

### 1. Install the Application

**Easy Way (Recommended):**
1. Download `NCDConverterSetup.msi`
2. Double-click to run installer
3. Follow installation wizard
4. Find "NCD Converter" on Desktop or Start Menu

### 2. Convert Your Files

1. **Launch** the application
2. **Select Files**: Click "Browse Files..." 
   - Navigate to: `C:\Users\{YourName}\Documents\NinjaTrader 8\db\minute\{Symbol}\`
   - Or: `...\db\tick\{Symbol}\`
   - Select one or more `.ncd` files
3. **Choose Output**: Click "Browse..." in Output Settings
   - Select where you want the `.parquet` files saved
4. **Convert**: Click "Convert to Parquet"
5. **Wait**: Watch the log for progress
6. **Done**: Find your `.parquet` files in the output folder

### 3. Use the Parquet Files

**In Python:**
```python
import pandas as pd
df = pd.read_parquet('20241021.parquet')
print(df.head())
```

---

## For Developers (Building from Source)

### Prerequisites

Install these first:
1. **Visual Studio 2022** (Community Edition is free)
   - Download: https://visualstudio.microsoft.com/downloads/
   - Workload: ".NET Desktop Development"
   
2. **.NET 6.0 SDK** (may be included with VS)
   - Download: https://dotnet.microsoft.com/download/dotnet/6.0

3. **WiX Toolset 3.11** (optional, for building installer)
   - Download: https://wixtoolset.org/releases/
   - Add to PATH: `C:\Program Files (x86)\WiX Toolset v3.11\bin`

### Build Steps

**Option A: Using Visual Studio**
1. Open `NCDConverter.sln`
2. Select "Release" configuration
3. Build → Build Solution (Ctrl+Shift+B)
4. Run: Debug → Start Without Debugging (Ctrl+F5)

**Option B: Using Command Line**
1. Open Command Prompt or PowerShell
2. Navigate to project folder:
   ```batch
   cd C:\path\to\NCDConverter
   ```
3. Run build script:
   ```batch
   build.bat
   ```
4. Executable will be in `.\publish\NCDConverter.exe`

**Option C: Create Installer**
1. First build the application (Option A or B)
2. Run:
   ```batch
   build-installer.bat
   ```
3. Installer will be in `.\Installer\NCDConverterSetup.msi`

### Project Structure

```
NCDConverter/
├── NCDConverter/              # Main application
│   ├── MainWindow.xaml       # UI layout
│   ├── MainWindow.xaml.cs    # Application logic
│   └── NCDConverter.csproj   # Project configuration
├── SharedLibs/               # NCD file readers
│   ├── NCDFile.cs           # Main parser
│   └── Rdx_System_IO.cs     # Binary helpers
├── Installer/               # WiX installer
│   └── Installer.wxs        # Installer config
├── build.bat                # Build script
├── build-installer.bat      # Installer script
└── README.md               # Full documentation
```

### Testing Your Build

1. Copy a few NCD files to a test folder
2. Run the application
3. Select the test NCD files
4. Choose an output folder
5. Click Convert
6. Verify Parquet files are created
7. Open in Python/Pandas to verify data

### Common Build Issues

**Issue**: "Cannot find dotnet"
- **Fix**: Install .NET 6.0 SDK and restart terminal

**Issue**: "Cannot find candle.exe" (installer build)
- **Fix**: Install WiX Toolset and add to PATH

**Issue**: "Parquet.Net not found"
- **Fix**: Run `dotnet restore` in project folder

**Issue**: NCD files from uploaded code not found
- **Fix**: Files are in `SharedLibs/` folder, linked in .csproj

### Customization

**Change Application Name:**
- Edit `NCDConverter.csproj` → `<Product>` and `<AssemblyName>`
- Edit `MainWindow.xaml` → `Title` attribute

**Change UI Colors:**
- Edit `MainWindow.xaml` → Modify `Background` and `Foreground` properties

**Add Features:**
- Edit `MainWindow.xaml.cs` → Add new methods
- Edit `MainWindow.xaml` → Add new UI controls

---

## Support

**User Issues:**
- Check conversion log for errors
- Verify NCD files open in NinjaTrader
- Ensure output folder has write permissions

**Developer Issues:**
- Verify all prerequisites installed
- Check build output for specific errors
- Ensure all NuGet packages restored

## Quick Commands Reference

```batch
# Restore packages
dotnet restore

# Build debug version
dotnet build

# Build release version
dotnet build -c Release

# Run the app
dotnet run --project NCDConverter/NCDConverter.csproj

# Publish standalone
dotnet publish -c Release -r win-x64 --self-contained false

# Full automated build
build.bat

# Build installer (requires WiX)
build-installer.bat
```

---

**Ready to go!** If you're a user, just run the installer. If you're a developer, run `build.bat` and start coding!
