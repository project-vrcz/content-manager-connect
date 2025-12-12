# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.2] - 2025-12-12

### Changed

- Mark Avatars pacakge as dependence to avoid confuse. [`#49`](https://github.com/project-vrcz/content-manager-connect/pull/49)

## [0.1.1] - 2025-12-11

### Added

- Smart pre upload check logic [`#40`](https://github.com/project-vrcz/content-manager-connect/pull/40)
  - Check Connection State:
    - Connected:
      - Check Is Connection Valid:
        - Valid: **Continue Upload**
        - Invalid: **Prevent upload and show dialog.** Disconnect (won't forget session).
    - Diconnected
      - Check Is Last Session Exist:
        - Exist:
          - Try Restore Last Session:
            - Success: **Continue Upload**
            - Failed: **Prevent upload and show dialog.**
        - No Exist:
          - **Prevent upload and show dialog.**

## [0.1.0] - 2025-12-08

### Added

- Show RPC Connection Status in CAU GUI.
- Check RPC Connection Status before CAU upload start
  - Prevent upload if disconnected and use content manager publish flow is enabled.

[unreleased]: https://github.com/project-vrcz/content-manager-connect/compare/cau-ext-v0.1.2...HEAD
[0.1.2]: https://github.com/project-vrcz/content-manager-connect/compare/cau-ext-v0.1.1...cau-ext-v0.1.2
[0.1.1]: https://github.com/project-vrcz/content-manager-connect/compare/cau-ext-v0.1.1...cau-ext-v0.1.1
[0.1.1]: https://github.com/project-vrcz/content-manager-connect/compare/cau-ext-v0.1.0...cau-ext-v0.1.1
[0.1.0]: https://github.com/project-vrcz/content-manager-connect/releases/tag/cau-ext-v0.1.0
