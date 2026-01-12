# Git Commit Analyzer - Ralph Loop Prompt

## Project Goal

Build a C# console application that analyzes a Git repository's commit history and displays beautiful statistics using Spectre.Console.

## Current State

Check `git log --oneline` and the existing code to understand what's been implemented. Build upon the existing foundation.

## Requirements

### Core Features (Implement in Order)

1. **Command Line Interface**

   - Accept optional path argument (default to current directory)
   - Add `--days` flag to limit analysis period (default: 90 days)
   - Add `--output` flag for JSON export capability
   - Use Spectre.Console.Cli for argument parsing

2. **Git Data Extraction**

   - Execute `git log` commands using System.Diagnostics.Process
   - Parse commit metadata: hash, author, date, message
   - Parse file changes: files modified, insertions, deletions
   - Handle edge cases: merge commits, empty commits

3. **Statistics Engine**

   - Commit frequency by day of week
   - Commit frequency by hour of day (heatmap style)
   - Most frequently edited files (top 10)
   - Author contribution breakdown
   - Average commits per day/week
   - Longest streak of consecutive commit days

4. **Visualization with Spectre.Console**

   - ASCII bar charts for frequency data
   - Tables for file and author statistics
   - Color-coded output based on activity levels
   - Progress spinner during analysis

5. **Polish**
   - Error handling for non-git directories
   - Helpful error messages
   - `--help` documentation
   - Unit tests for parsing logic

## Architecture Guidelines

```
GitCommitAnalyzer/
├── Program.cs              # Entry point with CLI setup
├── Commands/
│   └── AnalyzeCommand.cs   # Main analysis command
├── Services/
│   ├── GitService.cs       # Git command execution
│   └── StatsService.cs     # Statistics calculation
├── Models/
│   ├── Commit.cs           # Commit data model
│   └── AnalysisResult.cs   # Statistics result model
└── Rendering/
    └── ConsoleRenderer.cs  # Spectre.Console output
```

## Quality Checks

Before marking complete, verify:

- [ ] `dotnet build` succeeds with no warnings
- [ ] Running in a git repo produces meaningful output
- [ ] Running outside a git repo shows helpful error
- [ ] `--help` displays usage information
- [ ] Code has XML documentation comments

## Completion Signal

When ALL requirements are implemented and tests pass, output:

<promise>ANALYZER_COMPLETE</promise>

## Iteration Notes

If stuck or blocked:

- Document what's not working in a `BLOCKERS.md` file
- Try an alternative approach
- After 15 iterations without progress, output the blocker details and stop

## Remember

- Keep commits small and focused
- Write clean, idiomatic C# code
- Use async/await where appropriate
- Follow .NET naming conventions
