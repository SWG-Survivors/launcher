# Privacy Policy

## Network Communication

This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it.

## Data Collection

The SWGSurvivors Delta Patcher:

- **Does NOT collect** any personal information, telemetry, analytics, or usage statistics
- **Does NOT track** users or their behavior
- **Does NOT send** any data to third-party services

## Network Requests

The patcher makes network requests only for its core functionality:

1. **Checking for launcher updates** - Connects to GitHub Releases API to check for newer versions
   - URL: `https://api.github.com/repos/SWG-Survivors/launcher/releases/latest`
   - Purpose: Self-update capability
   - User control: User is prompted before any update is downloaded

2. **Downloading game files** - Connects to SWGSurvivors file server to download game patches
   - URL: `http://fileserver.swgsurvivors.com/swgs/updates/`
   - Purpose: Download missing or modified game files
   - User control: Only occurs when user clicks "Start Patching"

3. **Server status display** - Connects to SWGSurvivors server status endpoint
   - URL: `http://fileserver.swgsurvivors.com/status/server_status.php`
   - Purpose: Display real-time player count and server uptime
   - User control: Automatic on startup, timeout after 5 seconds if unavailable

## Third-Party Components

This software uses the following third-party components:

- **.NET 9.0 Runtime** (Microsoft)
  - Privacy policy: https://privacy.microsoft.com/
  - Embedded in self-contained executable
  - No separate data collection by this component

## User Control

Users have complete control over network operations:

- All update downloads require explicit user confirmation
- Game patching only occurs when user clicks "Start Patching"
- The program can be used offline (server status will show as unavailable)

## Changes to This Policy

Any changes to this privacy policy will be documented in version control and included in release notes.

## Questions

For questions about privacy or data handling, please open an issue at: https://github.com/SWG-Survivors/launcher/issues
