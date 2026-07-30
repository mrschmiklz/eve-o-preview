# Changelog

All notable changes to [this fork](https://github.com/mrschmiklz/eve-o-preview) are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

Download builds from [GitHub Releases](https://github.com/mrschmiklz/eve-o-preview/releases).

## [Unreleased]

## [8.0.5.5] - 2026-07-30

### Fixed

- EVE clients list crash: override `CheckedListBox.OnDrawItem` so the base class never calls the broken visual-styles checkbox renderer.

## [8.0.5.4] - 2026-07-30

### Fixed

- Crash in the EVE clients list (`CheckedListBox` visual-styles checkbox rendering failed under dark theme).

## [8.0.5.3] - 2026-07-30

### Fixed

- Crash when closing the settings window with minimize-to-tray enabled (input hooks disposed too early).
- Unstable combo box custom painting removed; safer dark-theme styling instead.
- Safer animation-style and version loading on startup.
- Crash log now written next to the executable (`EVE-O-Preview.log`).

## [8.0.5.2] - 2026-07-30

### Changed

- Rebuilt settings window for cycle-only use: single compact panel, no preview/thumbnail/zoom/overlay tabs.
- Dark theme for the main settings window with improved spacing and group box labels.
- Preview visibility is always off in this build; cycle bindings, client list, and tray options remain.

### Fixed

- Tab layout regression from flat-button styling; group box titles no longer clipped by borders.

## [8.0.5.1] - 2026-07-30

### Added

- Cycle-only default: hide all thumbnail windows by default; mouse cycle + minimize-all bindings unchanged.
- Thumbnail tab option to fully hide previews (distinct from color-block placeholders).
- Build-and-test script, unit tests, and CI verify workflow.
- `--smoke-test` flag for automated exe health checks.

### Changed

- Settings UI reorganized for cycle-first flow (Cycle tab); tighter window layout.
- Default config reset in repo root and example config.

### Fixed

- DWM thumbnail crash when prevent-previews and forced refresh interacted.
- Thumbnail refresh reentrancy and safer event-handler lookups during client attach/detach.
- Character-select cycling for clients titled `EVE`.

## [8.0.5.0] - 2026-07-26

### Changed

### Fixed

- Client cycling no longer leaves all EVE windows minimized when **Minimize inactive EVE clients** is enabled.
- Restores minimized clients before bringing them to the foreground.
- Improved Windows foreground handoff (`AttachThreadInput`, restore-before-activate).
- Mouse side-button actions now run on the UI thread.

## [8.0.4.5] - 2026-07-25

### Fixed

- Cycle-to-next swaps between logged-in clients instead of landing on the login screen.
- Existing configs upgrade with **Minimize inactive EVE clients** enabled by default (`ConfigVersion` 2).

## [8.0.4.4] - 2026-07-25

### Changed

- Default bindings: **Mouse 4** (XButton1) = cycle to next client, **Mouse 5** (XButton2) = minimize all clients.
- **Minimize inactive EVE clients** enabled by default for new configs.
- Global keyboard and mouse action bindings with active-client sync.

## [8.0.4.3] - 2026-07-25

### Fixed

- Client cycling matches live running EVE clients when saved cycle order is stale or uses example names.
- All configured cycle hotkeys register reliably via direct `HotkeyHandler` registration.

## [8.0.4.2] - 2026-07-25

### Fixed

- Cycle binding hotkeys fire at runtime (keyboard bindings work globally).

## [8.0.4.1] - 2026-07-25

### Added

- **General** tab → **Client action bindings**: click-to-capture keyboard or mouse buttons (replaces old mouse dropdown UI).
- Supports **Cycle to next client** and **Minimize all clients** bindings.

### Fixed

- Stability improvements for mouse-button cycling.

## [8.0.4.0] - 2026-07-25

### Added

- Configurable mouse-button client cycling (initial fork feature; G203 side-button workflow).
- `scripts/publish-release.ps1` for local Windows release builds on this fork.

[8.0.4.6]: https://github.com/mrschmiklz/eve-o-preview/releases/tag/8.0.4.6
[8.0.4.5]: https://github.com/mrschmiklz/eve-o-preview/releases/tag/8.0.4.5
[8.0.4.4]: https://github.com/mrschmiklz/eve-o-preview/releases/tag/8.0.4.4
[8.0.4.3]: https://github.com/mrschmiklz/eve-o-preview/releases/tag/8.0.4.3
[8.0.4.2]: https://github.com/mrschmiklz/eve-o-preview/releases/tag/8.0.4.2
[8.0.4.1]: https://github.com/mrschmiklz/eve-o-preview/releases/tag/8.0.4.1
[8.0.4.0]: https://github.com/mrschmiklz/eve-o-preview/releases/tag/8.0.4.0
