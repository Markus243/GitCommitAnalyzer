using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitCommitAnalyzer.Models;
using GitCommitAnalyzer.Rendering;
using GitCommitAnalyzer.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitCommitAnalyzer.Commands;

/// <summary>
/// Command settings for the analyze command.
/// </summary>
public class AnalyzeSettings : CommandSettings
{
    /// <summary>
    /// The path to the Git repository to analyze. Defaults to current directory.
    /// </summary>
    [Description("Path to the Git repository to analyze")]
    [CommandArgument(0, "[path]")]
    public string? Path { get; set; }

    /// <summary>
    /// Number of days to analyze (from today going back).
    /// </summary>
    [Description("Number of days to analyze (default: 90)")]
    [CommandOption("-d|--days")]
    [DefaultValue(90)]
    public int Days { get; set; } = 90;

    /// <summary>
    /// Path to output JSON file for export.
    /// </summary>
    [Description("Output path for JSON export (optional)")]
    [CommandOption("-o|--output")]
    public string? Output { get; set; }
}

/// <summary>
/// Main command for analyzing a Git repository.
/// </summary>
public class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    /// <summary>
    /// Executes the analysis command.
    /// </summary>
    public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings, CancellationToken cancellationToken)
    {
        var repoPath = settings.Path ?? Directory.GetCurrentDirectory();

        // Validate path exists
        if (!Directory.Exists(repoPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Directory not found: [yellow]{Markup.Escape(repoPath)}[/]");
            return 1;
        }

        var gitService = new GitService(repoPath);

        // Check if it's a Git repository
        var isGitRepo = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Checking Git repository...", async _ =>
            {
                return await gitService.IsGitRepositoryAsync();
            });

        if (!isGitRepo)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Not a Git repository: [yellow]{Markup.Escape(repoPath)}[/]");
            AnsiConsole.MarkupLine("[grey]Hint: Make sure you're in a directory with a .git folder, or specify a valid Git repository path.[/]");
            return 1;
        }

        // Validate days parameter
        if (settings.Days <= 0)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Days must be a positive number.");
            return 1;
        }

        List<Commit> commits = [];

        // Fetch commits with progress
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Analyzing repository...", async ctx =>
            {
                var progress = new Progress<string>(message =>
                {
                    ctx.Status(message);
                });

                commits = await gitService.GetCommitsAsync(settings.Days, progress);
            });

        if (commits.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No commits found[/] in the last {settings.Days} days.");
            return 0;
        }

        // Calculate statistics
        var statsService = new StatsService();
        var result = statsService.CalculateStatistics(
            commits,
            repoPath,
            gitService.GetRepositoryName(),
            settings.Days);

        // Render to console
        var renderer = new ConsoleRenderer();
        renderer.RenderResult(result);

        // Export to JSON if requested
        if (!string.IsNullOrWhiteSpace(settings.Output))
        {
            await ExportToJsonAsync(result, settings.Output);
        }

        return 0;
    }

    /// <summary>
    /// Exports the analysis result to a JSON file.
    /// </summary>
    private static async Task ExportToJsonAsync(AnalysisResult result, string outputPath)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var json = JsonSerializer.Serialize(result, options);
            await File.WriteAllTextAsync(outputPath, json);

            AnsiConsole.MarkupLine($"[green]JSON exported to:[/] [blue]{Markup.Escape(outputPath)}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error exporting JSON:[/] {Markup.Escape(ex.Message)}");
        }
    }
}
