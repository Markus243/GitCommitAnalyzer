# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-01-17

### Added

- Initial release of Git Commit Analyzer
- Commit frequency analysis by day of week and hour of day
- Most frequently edited files tracking (top 10)
- Author contribution breakdown with percentages
- Commit streak detection and tracking
- JSON export functionality with `--output` flag
- Configurable analysis period with `--days` flag
- Beautiful terminal output using Spectre.Console
- Heatmap visualization for hourly commit distribution
- Support for merge commit detection
- Binary file handling in statistics

### Technical

- Built with .NET 10.0
- Published as a dotnet global tool
- Comprehensive XML documentation
- Full test coverage with xUnit

[Unreleased]: https://github.com/Markus243/GitCommitAnalyzer/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/Markus243/GitCommitAnalyzer/releases/tag/v1.0.0
