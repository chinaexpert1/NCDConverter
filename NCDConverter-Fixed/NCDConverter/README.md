# NCD to Parquet Converter

A professional Windows desktop application for converting NinjaTrader proprietary NCD files to Parquet format.

## Features

- **Batch Conversion**: Select and convert multiple NCD files at once
- **Auto-Detection**: Automatically detects file type (Minute or Tick data)
- **Record Count Display**: Shows number of records before conversion starts
- **Real-time Logging**: Monitor conversion progress with detailed logs
- **Modern GUI**: Clean, user-friendly interface built with WPF
- **Cross-Compatible**: Output Parquet files work with Python/Pandas, Arrow, and other data tools

## System Requirements

- Windows 10 or later (64-bit)
- .NET 6.0 Runtime (included with installer)
- NinjaTrader 8 (for source NCD files)

## Installation

### Option 1: Using the Installer (Recommended)

1. Download `NCDConverterSetup.msi` from the releases page
2. Double-click the installer file
3. Follow the installation wizard
4. Desktop and Start Menu shortcuts will be created automatically

### Option 2: Build from Source

Requirements:
- Visual Studio 2022 or later
- .NET 6.0 SDK
- WiX Toolset v3.11 (for installer)

Steps:
```bash
# Clone or extract the source code
cd NCDConverter

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build -c Release

# Run the application
dotnet run --project NCDConverter/NCDConverter.csproj
```

## Usage

### Basic Workflow

1. **Launch the Application**
   - Use desktop shortcut or Start Menu

2. **Select Input Files**
   - Click "Browse Files..." button
   - Navigate to your NinjaTrader data folder:
     - Default: `C:\Users\{YourUsername}\Documents\NinjaTrader 8\db`
   - Select one or more `.ncd` files
   - Files can be from `\minute\` or `\tick\` subfolders

3. **Choose Output Location**
   - Click "Browse..." in Output Settings
   - Select destination folder for Parquet files

4. **Convert**
   - Click "Convert to Parquet" button
   - Monitor progress in the Conversion Log
   - Output files will have the same name as input with `.parquet` extension

### File Organization

**Input (NCD files):**
```
C:\Users\{User}\Documents\NinjaTrader 8\db\
├── minute\
│   └── QQQ\
│       ├── 20241021.ncd
│       ├── 20241022.ncd
│       └── ...
└── tick\
    └── QQQ\
        ├── 20241021.ncd
        └── ...
```

**Output (Parquet files):**
```
{YourOutputFolder}\
├── 20241021.parquet
├── 20241022.parquet
└── ...
```

### Output Schema

**Minute Data Parquet Schema:**
```
- timestamp (DateTimeOffset) - Bar timestamp
- open (double)              - Open price
- high (double)              - High price
- low (double)               - Low price
- close (double)             - Close price
- volume (long)              - Volume
```

**Tick Data Parquet Schema:**
```
- timestamp (DateTimeOffset) - Tick timestamp
- price (double)             - Trade price
- bid (double)               - Bid price
- ask (double)               - Ask price
- volume (long)              - Tick volume
```

## Integration with Python

Once converted, use the Parquet files in your Python workflows:

```python
import pandas as pd
import pyarrow.parquet as pq

# Read Parquet file
df = pd.read_parquet('20241021.parquet')

# Display data
print(df.head())

# Work with timestamps
df['timestamp'] = pd.to_datetime(df['timestamp'])
df.set_index('timestamp', inplace=True)

# Analyze
print(f"Total records: {len(df)}")
print(f"Date range: {df.index.min()} to {df.index.max()}")
print(f"Total volume: {df['volume'].sum():,.0f}")
```

## Troubleshooting

### Common Issues

**"No records found" error:**
- Ensure NCD file is not corrupted
- Verify file is from NinjaTrader 8 (not NT7)
- Check file permissions

**"Access denied" error:**
- Close NinjaTrader before conversion
- Run application as Administrator
- Check output folder permissions

**Conversion fails silently:**
- Check the Conversion Log for error messages
- Verify sufficient disk space for output files
- Ensure .NET 6.0 Runtime is installed

### Getting Help

1. Check the Conversion Log for detailed error messages
2. Verify NCD files are valid by opening in NinjaTrader first
3. Try converting a single file to isolate the issue

## Architecture

```
NCDConverter/
├── NCDConverter/              # Main WPF application
│   ├── MainWindow.xaml       # UI definition
│   ├── MainWindow.xaml.cs    # UI logic & conversion engine
│   ├── App.xaml              # Application entry
│   └── NCDConverter.csproj   # Project file
├── SharedLibs/               # NCD reading libraries
│   ├── NCDFile.cs           # Core NCD parser
│   ├── Rdx_System_IO.cs     # Binary I/O helpers
│   └── DateTimeExtensions.cs # Utilities
├── Installer/               # WiX installer project
│   └── Installer.wxs        # Installer definition
└── README.md               # This file
```

## Technologies Used

- **Framework**: .NET 6.0, WPF (Windows Presentation Foundation)
- **Libraries**: 
  - Parquet.Net 4.18.1 - Apache Parquet file format support
  - System.Windows.Forms - Folder browser dialog
- **Installer**: WiX Toolset 3.11

## License

This project uses the NCDFile library by John R. Stokka, licensed under MIT License.

The converter application is provided as-is for educational and commercial use.

## Compatibility

- **NinjaTrader**: NT8 (tested with NT 8.0.x)
- **Output Format**: Apache Parquet (compatible with Pandas, Arrow, Spark, etc.)
- **Windows**: Windows 10, Windows 11 (64-bit)

## Performance

Typical conversion speeds:
- Minute data: ~50,000 records/second
- Tick data: ~100,000 records/second

Performance varies based on:
- File size and record count
- Storage device (SSD recommended)
- Available RAM

## Version History

### Version 1.0.0 (Initial Release)
- Multi-file batch conversion
- Support for Minute and Tick NCD files
- Automatic file type detection
- Real-time conversion logging
- Record count preview
- Modern WPF interface
- MSI installer with shortcuts

## Credits

- NCD File Format Library: John R. Stokka (jrstokka@gmail.com)
- Application Development: Trading Tools Team
- Parquet.Net Library: Ivan Gavryliuk

## Contact & Support

For issues, suggestions, or contributions:
- Check the Conversion Log for error details
- Review this README for common solutions
- Consult NinjaTrader documentation for NCD file specifications

---

**Note**: This converter is designed specifically for NinjaTrader 8 NCD files. For other data sources or formats, different tools may be required.
