# Git Commit Analyzer

A C# console application that analyzes your Git repository's commit history and displays beautiful statistics.

**Built using the Ralph Wiggum technique** - iterative AI development with Claude Code.

## Features

- 📊 Commit frequency analysis (by day, week, hour)
- 📁 Most frequently edited files
- 👥 Author contribution breakdown
- 🔥 Commit streaks and patterns
- 🎨 Beautiful terminal output with Spectre.Console

## Requirements

- .NET 10.0 SDK
- Git installed and accessible in PATH

## Installation

```bash
git clone <your-repo>
cd git-commit-analyzer
dotnet build
```

## Usage

```bash
# Analyze current directory
dotnet run

# Analyze specific repository
dotnet run -- /path/to/repo

# Limit to last 30 days
dotnet run -- --days 30

# Export to JSON
dotnet run -- --output stats.json
```

## Development

This project was built iteratively using the [Ralph Wiggum plugin](https://github.com/anthropics/claude-code/tree/main/plugins/ralph-wiggum) for Claude Code.

### Running the Ralph Loop

```bash
# Install the plugin first
claude /plugin install ralph-wiggum@claude-plugins-official

# Run the development loop
/ralph-loop:ralph-loop "Build the Git Commit Analyzer according to PROMPT.md" \
  --max-iterations 25 \
  --completion-promise "ANALYZER_COMPLETE"
```

## License

MIT
