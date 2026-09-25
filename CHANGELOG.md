# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-09-25

### Changed

- Updated to the rebuilt `SaveVar<T>` of save-data. `SetValue` keeps the value in memory until
  `PushAsync`, as before.
- `PullAsync()` only runs when there is an unsaved value; it no longer reloads the file when
  nothing changed.
- `PullAsync(transport)` writes the pulled value to the file straight away.

## [1.0.2] - 2026-09-21

### Changed

- `Stat<T>.Transports` uses `SelectTypeAttribute` instead of `SerializeReferenceListAttribute`:
  the transport type is picked on each element, which also works in inspectors that ignore
  `PropertyAttribute.applyToCollection`, like SaintsField's `SaintsEditor`.

### Fixed

- The "Pull Async" context menu of `IntStat` pushed the stat instead of pulling it.

## [1.0.1] - 2026-09-21

### Added

- `fefek5.Stats.Runtime` assembly definition, so the package compiles when installed
  through the Package Manager.
- `fefek5.Stats.Samples` assembly definition for `SteamIntStatTransport`.

### Changed

- Stats types now live in the `fefek5.Stats.Runtime` assembly instead of `Assembly-CSharp`,
  and `SteamIntStatTransport` in `fefek5.Stats.Samples`. Transports serialized under
  `Assembly-CSharp` have to be re-added.

## [1.0.0] - 2026-09-18

### Added

- `Stat<T>` ScriptableObject that keeps its value in a `SaveVar<T>` and pushes it to a list
  of transports.
- `IntStat` and `FloatStat` assets.
- `StatTransport<T>` base class for pushing and pulling a stat to an external backend.
- `SteamIntStatTransport` sample that syncs an `IntStat` with a Steam Toys stat.
