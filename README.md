# SWGSurvivors Delta Patcher

A self-updating launcher and delta patcher for SWGSurvivors built with C#/.NET 9.0.

## Features

✅ **Self-updating** - Automatically checks GitHub for launcher updates on startup
✅ **Delta patching** - Only downloads missing/modified game files
✅ **SHA-256 verification** - Ensures file integrity
✅ **Live server status** - Real-time player count and server uptime
✅ **Smart validation** - Prevents patching in wrong directory
✅ **Locked file detection** - Warns if game is running
✅ **Dual progress bars** - Overall and per-file progress tracking
✅ **Color-coded activity log** - Easy to read status updates
✅ **Multiple patch runs** - No restart needed between patches
✅ **Embedded resources** - Logo included, no external files
✅ **Self-contained** - Includes .NET runtime (~40 MB)
✅ **Dynamic versioning** - Version displayed in window title

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
- **`npm run "build and deploy"`** - Build and deploy to game directory in one step
- **`npm run "create github version tag"`** - Interactively create and push version tag
- **`npm run checksum`** - Generate SHA256 checksum of built executable

### Alternative Build Methods

**Using batch script:**
```bash
build.bat
```

**Manual dotnet command:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
```

## Automated Releases

The project uses GitHub Actions to automatically build and release the launcher when version tags are pushed.

### Creating a Release

1. **Update version** in `SWGSurvivors-Patcher.csproj`:
   ```xml
   <Version>0.2.1</Version>
   ```

2. **Commit and push** your changes:
   ```bash
   git add SWGSurvivors-Patcher.csproj
   git commit -m "Bump version to 0.2.1"
   git push origin main
   ```

3. **Create and push tag** (interactive):
   ```bash
   npm run "create github version tag"
   # Enter: v0.2.1
   ```

4. **GitHub Actions automatically:**
   - Builds the self-contained executable
   - Creates a GitHub pre-release
   - Attaches `SWGSurvivors-Patcher.exe` as a release asset
   - Generates release notes from commits

5. **Launcher auto-updates:**
   - Users' launchers detect the new version
   - Download and apply updates automatically (with user confirmation)

### Manual Tag Creation

```bash
git tag v0.2.1
git push origin main
git push origin v0.2.1
```

### Release Workflow

The [release workflow](.github/workflows/release.yml) triggers on tags matching `v*` and:
- Runs on `windows-latest`
- Installs .NET 9.0
- Builds with same settings as `npm run build`
- Creates pre-release with auto-generated notes

## Distribution

### For End Users

Download from: [GitHub Releases](https://github.com/SWG-Survivors/launcher/releases/latest)

The release contains a single file: **`SWGSurvivors-Patcher.exe`** (~40 MB)

This file contains:
- The patcher application
- .NET 9.0 runtime (no installation required)
- Embedded logo image
- All resources

**No additional files or installation needed!**

### Browser Download Warnings

Modern browsers may warn about unsigned executables.

Browser may show insecure/unsafe download warning (click "Keep" / "Download anyway" depending on browser)

Windows may show SmartScreen security warnings or AV false positives (click "More info" → "Run anyway" or exclude from AV scan)

This is normal for unsigned executables. The code is open source and available for review, or you can build it locally. We are actively pursuing free code signing through SignPath Foundation (see [CODE_SIGNING_POLICY.md](CODE_SIGNING_POLICY.md)).

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
launcher/
├── .github/
│   └── workflows/
│       └── release.yml              # GitHub Actions release workflow
├── SWGSurvivors-Patcher.csproj      # Project configuration
├── Program.cs                        # Entry point & self-update logic
├── MainForm.cs                       # Main UI and patcher logic
├── GitHubUpdateService.cs            # GitHub API integration for updates
├── UpdateForm.cs                     # Update progress UI
├── package.json                      # npm scripts
├── build.bat                         # Build script
├── logo.png                          # Embedded logo
├── swgs.ico                          # Application icon
├── CODE_SIGNING_POLICY.md            # Code signing policy and team roles
├── PRIVACY.md                        # Privacy policy
├── README.md                         # This file
├── .gitignore                        # Git ignore rules
└── publish/                          # Build output (git ignored)
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

## Code Signing

The SWGSurvivors launcher is currently unsigned but we are pursuing free code signing through the [SignPath Foundation](https://signpath.org) program for open source projects.

**Current Status**: Application in progress

**Benefits of code signing:**
- Eliminates most antivirus false positives
- Removes Windows SmartScreen warnings
- Provides authenticity verification for users
- Ensures executable integrity

For details on our code signing policy and privacy practices:
- [Code Signing Policy](CODE_SIGNING_POLICY.md)
- [Privacy Policy](PRIVACY.md)

Until code signing is implemented, users may see browser download warnings and Windows SmartScreen alerts. This is normal for unsigned executables. The code is open source and available for review.

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

## Self-Update Architecture

The launcher implements a sophisticated self-update system:

1. **Pre-launch Check**: On startup, checks GitHub Releases API for newer versions
2. **User Confirmation**: Prompts user before downloading updates
3. **Download**: Downloads new `.exe` as `.exe.new`
4. **Batch Script Replacement**: Creates temporary batch script to:
   - Wait for current process to exit
   - Delete old executable
   - Rename `.new` to original name
   - Restart launcher
   - Self-delete
5. **Seamless Update**: User sees brief restart, no manual intervention

### Update Safety Features

- **5-second timeout**: Won't hang on network issues
- **Version comparison**: Semantic versioning (Major.Minor.Build)
- **Graceful fallback**: Continues with current version if update fails
- **User control**: Always prompts before downloading
- **Responsive UI**: Update form remains interactive during download

## Version History

### v0.2.1
- Add dynamic version display in window title
- Retrieve version from assembly metadata at runtime

### v0.2.0
- **Self-update capability** - Automatic updates from GitHub Releases
- **GitHub Actions CI/CD** - Automated build and release pipeline
- **Live server status** - Real-time player count and uptime display
- **Smart validation** - Prevents patching in wrong directory
- **Improved UX** - Responsive async operations with proper timeout handling

### v0.1.1
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

## Technical Details

### API Endpoints

- **Manifest**: `http://fileserver.swgsurvivors.com/swgs/updates/manifest.json`
- **Server Status**: `http://fileserver.swgsurvivors.com/status/server_status.php`
- **GitHub Releases**: `https://api.github.com/repos/SWG-Survivors/launcher/releases/latest`

### Technologies

- **.NET 9.0** - Modern .NET runtime with Windows Forms
- **HttpClient** - Async HTTP operations with timeout handling
- **SHA256** - Cryptographic hash verification
- **JSON** - Manifest and API response parsing
- **Batch Scripts** - Self-replacement mechanism

### Security Features

- Hash verification for all downloaded files
- HTTPS for GitHub API (HTTP for internal endpoints with hash verification)
- No elevation required (runs as standard user)
- Temporary files cleaned up after updates

## Support & Issues

For issues or questions:
1. Check the Activity Log in the patcher for error details
2. Verify manifest is accessible: http://fileserver.swgsurvivors.com/swgs/updates/manifest.json
3. Check server status: http://fileserver.swgsurvivors.com/status/server_status.php
4. Ensure game is closed before patching
5. Try running as Administrator if permission errors occur
6. Report issues: https://github.com/SWG-Survivors/launcher/issues

## License

ISC License - See [LICENSE](LICENSE) file for details.
