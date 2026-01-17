# Git Commit Analyzer

[![NuGet](https://img.shields.io/nuget/v/GitCommitAnalyzer.svg)](https://www.nuget.org/packages/GitCommitAnalyzer/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GitCommitAnalyzer.svg)](https://www.nuget.org/packages/GitCommitAnalyzer/)
[![CI](https://github.com/Markus243/GitCommitAnalyzer/actions/workflows/ci.yml/badge.svg)](https://github.com/Markus243/GitCommitAnalyzer/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A powerful CLI tool that analyzes your Git repository's commit history and displays beautiful statistics.

## Features

- **Commit Frequency Analysis** - See when you're most productive by day of week and hour
- **File Hotspots** - Identify your most frequently edited files
- **Author Contributions** - Breakdown of commits, insertions, and deletions per contributor
- **Streak Tracking** - Find your longest consecutive commit streaks
- **Beautiful Output** - Rich terminal output with colors, tables, and heatmaps
- **JSON Export** - Export analysis results for further processing

## Installation

### As a .NET Global Tool (Recommended)

```bash
dotnet tool install -g GitCommitAnalyzer
```

After installation, you can use the `git-analyze` command from anywhere.

### From Source

```bash
git clone https://github.com/Markus243/GitCommitAnalyzer.git
cd GitCommitAnalyzer/GitCommitAnalyzer
dotnet build
```

## Usage

### Basic Usage

```bash
# Analyze current directory (last 90 days by default)
git-analyze

# Analyze a specific repository
git-analyze /path/to/repo
```

### Options

```bash
# Analyze last 30 days only
git-analyze --days 30

# Export results to JSON
git-analyze --output stats.json

# Combine options
git-analyze /path/to/repo --days 60 --output analysis.json
```

### Running from Source

```bash
# From the GitCommitAnalyzer directory
dotnet run -- /path/to/repo --days 30
```

## Example Output

```
╭──────────────────────────────────────────────────────────────╮
│  Git Commit Analyzer                                          │
│  Repository: my-project                                       │
│  Period: 2025-10-19 to 2026-01-17 (90 days)                  │
╰──────────────────────────────────────────────────────────────╯

──────────────────────── Summary ────────────────────────

  Total Commits:       247
  Merge Commits:       23
  Lines Added:         12,847
  Lines Deleted:       4,523
  Active Days:         68
  Longest Streak:      12 days

──────────────────── Commits by Day ─────────────────────

  Monday      ████████████████████████  52
  Tuesday     ██████████████████        38
  Wednesday   ████████████████████████  48
  ...
```

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Git installed and accessible in PATH

## Uninstall

```bash
dotnet tool uninstall -g GitCommitAnalyzer
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Development

This project was built using the [Ralph Wiggum technique](https://github.com/anthropics/claude-code/tree/main/plugins/ralph-wiggum) - an iterative AI development approach with Claude Code.

### Running Tests

```bash
cd GitCommitAnalyzer
dotnet test
```

### Building

```bash
cd GitCommitAnalyzer
dotnet build --configuration Release
```

### Creating a NuGet Package

```bash
cd GitCommitAnalyzer/GitCommitAnalyzer
dotnet pack --configuration Release
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a list of changes.
