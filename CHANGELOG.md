# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2024-11-30

### Added

- New sort options for the trash rules menu, with alphabetical instead of random default.

### Fixed

- Scrolling works correctly for long grids of trashable items/rules.

### Changed

- Migrate from self-hosted UI to [StardewUI](https://www.nexusmods.com/stardewvalley/mods/28870) (Framework).
- Slightly more informative tooltips, including sell value.
- Skip loading/saving mod data for farmhands in co-op/network play, which aren't compatible and may crash.
- Deprecated methods in Stardew 1.6.9+ replaced with current versions.

## [0.1.1] - 2024-08-02

### Fixed

- Trash filters now carry over properly between different levels of the same mine or volcano dungeon.

## [0.1.0] - 2024-08-02

### Added

- Initial release.
- Mark items as trash with modifier key in regular menus.
- Automatic discarding (per location) of trash items, with expected refund.
- New/replaced HUD notifications for discarded items.
- Trash menu for managing local/global trash items.
- Recovery menu and timed recovery.
- Save game persistence.
- [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) pages for global settings like key bindings and notifications.

[Unreleased]: https://github.com/focustense/StardewAutoTrash/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/focustense/StardewAutoTrash/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/focustense/StardewAutoTrash/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/focustense/StardewAutoTrash/tree/v0.1.0