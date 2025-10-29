# SWGSurvivors Delta Patcher (C# Version)

This is a C#/.NET rewrite of the Python patcher with significantly better antivirus detection rates.

## Features

✅ Delta patching - only downloads missing/modified files
✅ SHA-256 hash verification for file integrity
✅ Locked file detection (warns if game is running)
✅ Progress tracking with dual progress bars
✅ Color-coded activity log
✅ Multiple patch runs without restarting
✅ Embedded logo (no external files needed)
✅ Self-contained executable (~40 MB, includes .NET runtime)

## Why C# Version?

- **Native Windows application** - AV trusts .NET executables
- **Very low false positive rate** - Microsoft technology is trusted (0-3 AV engines vs 10-20 for Python)
- **Self-contained** - Includes .NET runtime, no dependencies needed
- **Better performance** - Native compiled code
- **Professional appearance** - Native Windows UI
- **Single executable** - Logo and all resources embedded

## Prerequisites

- .NET 9.0 SDK or higher
- Windows 10/11

### Installing .NET SDK

Download from: https://dotnet.microsoft.com/download/dotnet/9.0

Or check if already installed:
```bash
dotnet --version
```

## Building

### Quick Start

```bash
npm run build
```

The executable will be in `publish/SWGSurvivors-Patcher.exe`

### Available npm Scripts

- **`npm run build`** - Build the patcher
- **`npm run clean`** - Remove build artifacts (bin/, obj/, publish/)
- **`npm run rebuild`** - Clean and rebuild from scratch
- **`npm run test`** - Run in development mode (no build required)
- **`npm run move`** - Copy built .exe to game directory
- **`npm run build:deploy`** - Build and deploy to game directory in one step

### Alternative Build Methods

**Using batch script:**
```bash
build.bat
```

**Manual dotnet command:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
```

## Distribution

### For End Users

Distribute only: **`publish/SWGSurvivors-Patcher.exe`** (~40 MB)

This single file contains:
- The patcher application
- .NET 9.0 runtime (no installation required)
- Embedded logo image
- All resources

**No additional files needed!**

### Browser Download Warnings

Modern browsers may warn about unsigned executables. Options:

1. **Zip the file** (reduces warnings):
   ```bash
   cd publish
   powershell Compress-Archive -Path SWGSurvivors-Patcher.exe -DestinationPath SWGSurvivors-Patcher.zip
   ```

2. **Provide user instructions:**
   - Chrome: Click "Keep" → "Keep anyway"
   - Edge: Click "..." → "Keep"
   - Windows SmartScreen: Click "More info" → "Run anyway"

3. **Code signing** (eliminates warnings, costs $100-400/year):
   - Purchase certificate from DigiCert, Sectigo, or GlobalSign
   - See "Code Signing" section below

## Advantages Over Python Version

| Feature | Python (PyInstaller) | C# (.NET) |
|---------|---------------------|-----------|
| AV Detection Rate | High (10-20 engines) | Very Low (0-3 engines) |
| File Size | ~10 MB | ~40 MB |
| Build Time | 30 seconds | 1-2 minutes |
| Dependencies | None | None |
| Performance | Good | Excellent |
| Trust Level | Low (packed runtime) | High (Microsoft .NET) |

## Testing Before Distribution

1. Build the executable
2. Upload to VirusTotal.com
3. Check detection rates (should be 0-3 detections)
4. Test on a clean Windows VM

## Distribution

Distribute only:
```
publish/SWGSurvivors-Patcher.exe
```

This single file is completely self-contained and requires no installation.

## Development

### Project Structure

```
csharp-version/
├── SWGSurvivors-Patcher.csproj  # Project configuration
├── Program.cs                    # Entry point
├── MainForm.cs                   # Main UI and logic (~600 lines)
├── package.json                  # npm scripts
├── build.bat                     # Build script
├── logo.png                      # Embedded logo (80x80 recommended)
├── swgs.ico                      # Application icon
├── README.md                     # This file
├── .gitignore                    # Git ignore rules
└── publish/                      # Build output (git ignored)
    └── SWGSurvivors-Patcher.exe
```

### Running in Development

```bash
npm run test
# or
dotnet run
```

### Modifying the Embedded Logo

1. Replace `logo.png` with your image (square dimensions recommended)
2. Rebuild: `npm run rebuild`
3. Logo is automatically embedded in the .exe

### Modifying the Application Icon

1. Replace `swgs.ico` with your icon file
2. Rebuild: `npm run rebuild`
3. Icon appears in taskbar and file explorer

## Code Signing (Optional but Recommended)

For even better AV detection and user trust, sign the executable:

```bash
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com publish\SWGSurvivors-Patcher.exe
```

Code signing certificates cost $100-400/year but eliminate almost all antivirus warnings.

## Troubleshooting

### ".NET SDK not found"

Install .NET 9.0 SDK from https://dotnet.microsoft.com/download/dotnet/9.0

### "Build failed"

Make sure you're in the correct directory:
```bash
cd csharp-version
dotnet restore
dotnet build
```

### "Application won't run"

The self-contained build should work on any Windows 10/11 system. If it doesn't:
1. Make sure you're on Windows 10/11
2. Try rebuilding
3. Check antivirus didn't quarantine it

## Comparison with Python Version

Both versions have identical functionality. Choose based on your priorities:

| Feature | Python (PyInstaller) | C# (.NET) |
|---------|---------------------|-----------|
| **AV Detection Rate** | High (10-20 engines) ⚠️ | Very Low (0-3 engines) ✅ |
| **File Size** | ~10 MB | ~40 MB |
| **Build Time** | 30 seconds | 1-2 minutes |
| **Browser Warnings** | Yes (unsigned) | Yes (unsigned) |
| **Dependencies** | None | None |
| **Performance** | Good | Excellent |
| **Code Modifications** | Easier (Python) | Requires C# knowledge |
| **Trust Level** | Low (packed runtime) | High (Microsoft .NET) ✅ |
| **Embedded Resources** | Possible | Native support ✅ |

**Recommendation:** Use the **C# version** for distribution to users due to significantly lower antivirus false positives.

## Version History

### v1.0.0
- Initial C# port from Python version
- All features from Python version implemented
- Embedded logo support
- Multi-run capability (no restart needed)
- Improved button behavior
- Fixed working directory detection
- Reset statistics between runs

## Known Limitations

- **Browser warnings** - Unsigned executables trigger SmartScreen (applies to both Python and C# versions)
- **File size** - Larger than Python version due to embedded .NET runtime
- **Windows only** - No Mac/Linux support (use Python version for cross-platform)

## Support & Issues

For issues or questions:
1. Check the Activity Log in the patcher for error details
2. Verify manifest is accessible: http://fileserver.swgsurvivors.com/swgs/updates/manifest.json
3. Ensure game is closed before patching
4. Try running as Administrator if permission errors occur
