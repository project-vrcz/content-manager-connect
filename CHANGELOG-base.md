# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Smart pre upload check logic (For CAU Extension) [`#40`](https://github.com/project-vrcz/content-manager-connect/pull/40)

### Changed

- Auto uppercase when enter Challenge Code. [`#39`](https://github.com/project-vrcz/content-manager-connect/pull/39)
- Brand new Settings UI. [`#37`](https://github.com/project-vrcz/content-manager-connect/pull/37)
  - Better looking.
  - Avoid confuse caused by old UI (e.g accidently forget App instance when reconnect failed).

## [0.1.2] - 2025-12-09

### Changed

- Rename to `VRChat Content Manager Connect - Base` to avoid confuse. [`#33`](https://github.com/project-vrcz/content-manager-connect/pull/33)

### Added

- Show Warning if both Avatars and Worlds Connect Packages are not installed. [`#31`](https://github.com/project-vrcz/content-manager-connect/pull/31)

## [0.1.1] - 2025-12-08

### Added

- Expose interal type to `xyz.misakal.vpm.vcm-connect.avatars.continuous-avatar-uploader-ext`

## [0.1.0] - 2025-12-08

### Added

- Ability to Connect to VRChat Content Manager App.
- Allow Create new Content. [`#17`](https://github.com/project-vrcz/content-manager-connect/pull/17)
- Allow restore last session. [`#10`](https://github.com/project-vrcz/content-manager-connect/pull/10)
- Show current connected VRChat Content Manager Instance Name. [`#6`](https://github.com/project-vrcz/content-manager-connect/pull/6)
- Allow custom Client Name. [`#13`](https://github.com/project-vrcz/content-manager-connect/pull/13)
- Cancel CAU Upload if use content manager publish flow enabled, and RPC connection is disconnected. [`#14`](https://github.com/project-vrcz/content-manager-connect/pull/14)

## Fixed

- Fix unable to build and upload when use Content Mangaer publish flow is disbaled. [`#8`](https://github.com/project-vrcz/content-manager-connect/pull/8)
- Fix Connect Settings show upgrade sdk warning in non-avatar project. [`#24`](https://github.com/project-vrcz/content-manager-connect/pull/24)

## [0.1.0-beta.1] - 2025-12-07

### Added

- Ability to Connect to VRChat Content Manager App.
- Allow Create new Content. [`#17`](https://github.com/project-vrcz/content-manager-connect/pull/17)
- Allow restore last session. [`#10`](https://github.com/project-vrcz/content-manager-connect/pull/10)
- Show current connected VRChat Content Manager Instance Name. [`#6`](https://github.com/project-vrcz/content-manager-connect/pull/6)
- Allow custom Client Name. [`#13`](https://github.com/project-vrcz/content-manager-connect/pull/13)
- Cancel CAU Upload if use content manager publish flow enabled, and RPC connection is disconnected. [`#14`](https://github.com/project-vrcz/content-manager-connect/pull/14)

## Fixed

- Fix unable to build and upload when use Content Mangaer publish flow is disbaled. [`#8`](https://github.com/project-vrcz/content-manager-connect/pull/8)

[unreleased]: https://github.com/project-vrcz/content-manager-connect/compare/base-v0.1.2...HEAD
[0.1.2]: https://github.com/project-vrcz/content-manager-connect/compare/base-v0.1.1...base-v0.1.2
[0.1.1]: https://github.com/project-vrcz/content-manager-connect/compare/base-v0.1.0...base-v0.1.1
[0.1.0]: https://github.com/project-vrcz/content-manager-connect/compare/base-v0.1.0-beta.1...base-v0.1.0
[0.1.0-beta.1]: https://github.com/project-vrcz/content-manager-connect/releases/tag/base-v0.1.0-beta.1