# Changelog

All notable changes to the NCD to Parquet Converter project will be documented in this file.

## [1.0.0] - 2024-10-24

### Added
- Initial release of NCD to Parquet Converter
- Multi-file batch conversion support
- Automatic file type detection (Minute vs Tick)
- Real-time conversion progress logging
- Record count preview before conversion
- Modern WPF user interface with clean design
- Support for NinjaTrader 8 NCD files
- Parquet output format compatible with Python/Pandas
- MSI installer with desktop and start menu shortcuts
- Comprehensive README and Quick Start guide
- Build scripts for Windows (batch and PowerShell)
- Installer build script using WiX Toolset

### Features
- **Input Support**: 
  - Minute bar NCD files
  - Tick data NCD files
  - Batch processing of multiple files
  
- **Output Format**:
  - Apache Parquet format
  - Maintains original filename with .parquet extension
  - Schema includes: timestamp, OHLC (minute), price/bid/ask (tick), volume

- **User Interface**:
  - File browser for multi-select
  - Output folder selection
  - Real-time conversion log
  - Progress indication
  - Clear log functionality

- **Developer Features**:
  - .NET 6.0 WPF application
  - Parquet.Net integration
  - NCDFile library integration
  - Clean architecture with separated concerns
  - Comprehensive error handling

### Technical Details
- Built with .NET 6.0
- Uses Parquet.Net 4.18.1
- WPF for Windows desktop application
- WiX Toolset 3.11 for installer creation
- Compatible with Windows 10/11 64-bit

### Documentation
- Complete README with usage instructions
- Quick Start guide for users and developers
- Architecture overview
- Python integration examples
- Troubleshooting section
- Build instructions

---

## Roadmap / Future Versions

### [1.1.0] - Planned
- [ ] Drag-and-drop file support
- [ ] Conversion presets/templates
- [ ] Automatic NinjaTrader path detection
- [ ] Recent files list
- [ ] Settings persistence

### [1.2.0] - Planned
- [ ] CSV output option
- [ ] Data preview before conversion
- [ ] Filter/date range selection
- [ ] Compression options for Parquet
- [ ] Multi-threaded conversion

### [2.0.0] - Future
- [ ] Day bar support
- [ ] Custom schema mapping
- [ ] Data validation and repair
- [ ] Command-line interface
- [ ] Scheduled conversions
- [ ] Cloud storage integration

---

## Version Numbering

This project follows Semantic Versioning (SemVer):
- MAJOR version for incompatible API changes
- MINOR version for backwards-compatible functionality additions
- PATCH version for backwards-compatible bug fixes

## Contributing

We welcome contributions! Please check the project repository for:
- Bug reports
- Feature requests
- Code contributions
- Documentation improvements

## Support

For issues or questions:
- Check the README for common solutions
- Review this changelog for recent changes
- Consult the Quick Start guide
