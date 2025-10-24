# Deployment Guide

This guide explains how to build and distribute the NCD to Parquet Converter application.

## Prerequisites for Building

### Required Software
1. **Windows 10/11** (64-bit)
2. **.NET 6.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/6.0)
3. **Visual Studio 2022** (optional but recommended) - [Download](https://visualstudio.microsoft.com/)
4. **WiX Toolset 3.11+** (for installer) - [Download](https://wixtoolset.org/releases/)

### Verify Installation
```batch
dotnet --version
# Should show: 6.0.x or higher

candle.exe /?
# Should show WiX compiler help (if building installer)
```

## Building the Application

### Method 1: Automated Build (Recommended)

**Windows Batch:**
```batch
cd NCDConverter
build.bat
```

**PowerShell:**
```powershell
cd NCDConverter
.\build.ps1
```

This will:
- Clean previous builds
- Restore NuGet packages
- Build in Release mode
- Publish to `./publish/` folder

**Output:** `./publish/NCDConverter.exe`

### Method 2: Manual Build

```batch
# Navigate to project
cd NCDConverter

# Restore packages
dotnet restore NCDConverter.sln

# Build
dotnet build NCDConverter.sln -c Release

# Publish
dotnet publish NCDConverter/NCDConverter.csproj -c Release -r win-x64 --self-contained false -o ./publish
```

### Method 3: Visual Studio

1. Open `NCDConverter.sln`
2. Select **Release** configuration
3. Right-click solution → **Build Solution**
4. Find output in `NCDConverter/bin/Release/net6.0-windows/`

## Creating the Installer

### Prerequisites
- Application must be built first (see above)
- WiX Toolset installed and in PATH

### Build Installer

**Using Batch Script:**
```batch
build-installer.bat
```

**Manual Build:**
```batch
cd Installer
candle.exe Installer.wxs -ext WixUIExtension
light.exe Installer.wixobj -ext WixUIExtension -out NCDConverterSetup.msi
cd ..
```

**Output:** `Installer/NCDConverterSetup.msi`

## Distribution Options

### Option 1: MSI Installer (Recommended)
- **File:** `NCDConverterSetup.msi`
- **Size:** ~5-10 MB
- **Benefits:** 
  - Professional installation experience
  - Creates Start Menu shortcuts
  - Creates Desktop shortcut
  - Adds to Programs & Features
  - Clean uninstall
- **Requirements:** Users need .NET 6.0 Runtime

**Distribution:**
1. Share `NCDConverterSetup.msi` via:
   - Download link
   - Network share
   - Email (if under size limit)
   - USB drive
2. Users double-click to install
3. Automatically creates shortcuts

### Option 2: Portable ZIP
- **File:** Package `./publish/` folder contents
- **Benefits:**
  - No installation required
  - Can run from USB drive
  - No admin rights needed
- **Requirements:** Users need .NET 6.0 Runtime

**Create Portable Package:**
```batch
# After building
cd publish
# Create ZIP file containing all files
```

**Distribution:**
1. Zip the `publish` folder
2. Share the ZIP file
3. Users extract and run `NCDConverter.exe`

### Option 3: Self-Contained (No Runtime Required)
- **Benefits:** Users don't need .NET installed
- **Drawback:** Much larger file size (~70-100 MB)

**Build Self-Contained:**
```batch
dotnet publish NCDConverter/NCDConverter.csproj -c Release -r win-x64 --self-contained true -o ./publish-standalone
```

## Version Management

### Update Version Number

Edit `NCDConverter/NCDConverter.csproj`:
```xml
<PropertyGroup>
    <Version>1.0.0</Version>  <!-- Change this -->
    ...
</PropertyGroup>
```

Also update:
- `Installer/Installer.wxs` - Product Version
- `CHANGELOG.md` - Add new version entry
- `README.md` - Update version references

### Creating a Release

1. Update version in all files
2. Update CHANGELOG.md
3. Build application
4. Build installer
5. Test installation on clean machine
6. Create release package

**Release Package Contents:**
```
NCDConverter-v1.0.0/
├── NCDConverterSetup.msi        # Installer
├── README.md                    # Documentation
├── QUICKSTART.md               # Quick guide
├── LICENSE.txt                 # License
└── CHANGELOG.md                # Version history
```

## Testing Before Distribution

### Test Checklist
- [ ] Clean Windows 10 VM test
- [ ] Clean Windows 11 VM test
- [ ] Install from MSI
- [ ] Desktop shortcut works
- [ ] Start Menu shortcut works
- [ ] Convert sample NCD files
- [ ] Verify Parquet output in Python
- [ ] Uninstall completely
- [ ] Check no leftover files
- [ ] Test with .NET 6.0 Runtime only (no SDK)

### Test Files Needed
- Sample Minute NCD files
- Sample Tick NCD files
- Python script to verify Parquet output

### Automated Test Script
```python
import pandas as pd
import os

def test_parquet(file_path):
    """Verify Parquet file integrity"""
    try:
        df = pd.read_parquet(file_path)
        print(f"✓ {file_path}")
        print(f"  Records: {len(df)}")
        print(f"  Columns: {list(df.columns)}")
        print(f"  Date range: {df['timestamp'].min()} to {df['timestamp'].max()}")
        return True
    except Exception as e:
        print(f"✗ {file_path}: {e}")
        return False

# Test all parquet files in output folder
output_dir = "C:\\path\\to\\output"
for file in os.listdir(output_dir):
    if file.endswith('.parquet'):
        test_parquet(os.path.join(output_dir, file))
```

## Deployment Environments

### End Users (Trading Desks)
- **Method:** MSI Installer
- **Requirements:** .NET 6.0 Runtime
- **Support:** README + Quick Start

### Corporate Deployment
- **Method:** MSI + Group Policy or SCCM
- **Requirements:** .NET 6.0 Runtime pre-installed
- **Configuration:** Customize `App.config` for network paths

### Developer Distribution
- **Method:** Source code + build instructions
- **Requirements:** .NET 6.0 SDK + Visual Studio
- **Documentation:** README + Architecture docs

## Troubleshooting Deployment

### Common Issues

**"Application requires .NET Runtime"**
- Install .NET 6.0 Runtime: https://dotnet.microsoft.com/download/dotnet/6.0
- Or build self-contained version

**"MSI installation fails"**
- Run as Administrator
- Check Windows Event Viewer for details
- Verify WiX built correctly
- Check disk space

**"Application won't start"**
- Check .NET version: `dotnet --version`
- Verify Windows version (Win 10+ required)
- Check Application Event Log

**"Can't find NCD files"**
- Verify NinjaTrader 8 data path
- Check folder permissions
- Ensure NCD files not open in NT

## Support Materials

Include with distribution:
1. **README.md** - Full documentation
2. **QUICKSTART.md** - Getting started guide
3. **LICENSE.txt** - Legal information
4. **CHANGELOG.md** - Version history

Optional:
5. Sample NCD files (if licensing allows)
6. Python verification script
7. Video tutorial
8. FAQ document

## Continuous Deployment

### Automated Build Pipeline (Future)

```yaml
# Example GitHub Actions workflow
name: Build and Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 6.0.x
      - name: Build
        run: dotnet build -c Release
      - name: Publish
        run: dotnet publish -c Release
      - name: Create installer
        run: build-installer.bat
      - name: Upload artifacts
        uses: actions/upload-artifact@v2
        with:
          name: NCDConverterSetup
          path: Installer/NCDConverterSetup.msi
```

## Security Considerations

### Code Signing (Recommended for Production)
- Obtain code signing certificate
- Sign EXE and MSI files
- Prevents Windows SmartScreen warnings

**Sign Files:**
```batch
signtool sign /f certificate.pfx /p password /t http://timestamp.server.com NCDConverter.exe
signtool sign /f certificate.pfx /p password /t http://timestamp.server.com NCDConverterSetup.msi
```

### Virus Scanning
- Scan files before distribution
- Submit to VirusTotal
- Include hash checksums

**Generate Checksums:**
```batch
certutil -hashfile NCDConverterSetup.msi SHA256
```

## Distribution Channels

1. **Direct Download:** Host MSI on website
2. **GitHub Releases:** Tag releases with artifacts
3. **Network Share:** Corporate internal distribution
4. **Microsoft Store:** (Future) Wider distribution
5. **Chocolatey:** (Future) Package manager

---

## Quick Deployment Checklist

- [ ] Update version numbers
- [ ] Update documentation
- [ ] Build application (`build.bat`)
- [ ] Build installer (`build-installer.bat`)
- [ ] Test on clean VM
- [ ] Create release package
- [ ] Generate checksums
- [ ] Sign files (if applicable)
- [ ] Upload to distribution channel
- [ ] Notify users
- [ ] Update changelog

---

**Ready to Deploy!** Follow this guide to build and distribute the NCD Converter to your users.
