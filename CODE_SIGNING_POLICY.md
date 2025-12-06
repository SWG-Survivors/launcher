# Code Signing Policy

Free code signing provided by [SignPath.io](https://signpath.io), certificate by [SignPath Foundation](https://signpath.org).

## Project Status

This project is currently in the process of applying for free code signing through the SignPath Foundation program for open source projects.

## Team Roles

Once approved, the following team structure will be used for code signing:

- **Authors**: Contributors who can submit code changes
  - [SWG-Survivors Organization](https://github.com/SWG-Survivors)
- **Reviewers**: Team members who review and approve pull requests
  - To be defined upon SignPath approval
- **Approvers**: Designated maintainers authorized to approve code signing requests
  - To be defined upon SignPath approval

## Policy

All releases of the SWGSurvivors Delta Patcher distributed through GitHub Releases will be code signed to ensure:

1. **Authenticity** - Users can verify the software comes from SWG-Survivors
2. **Integrity** - The executable has not been tampered with since signing
3. **Trust** - Reduced antivirus false positives and Windows SmartScreen warnings

Code signing will only be applied to:
- Official releases tagged with version numbers (e.g., `v0.2.1`)
- Executables built through our automated GitHub Actions CI/CD pipeline
- Code that has been reviewed and approved by project maintainers

## Privacy

See [PRIVACY.md](PRIVACY.md) for our privacy policy.
