using GitCommitAnalyzer.Models;
using Spectre.Console;

namespace GitCommitAnalyzer.Rendering;

/// <summary>
/// Renders analysis results using Spectre.Console for beautiful terminal output.
/// </summary>
public class ConsoleRenderer
{
    /// <summary>
    /// Renders the complete analysis result to the console.
    /// </summary>
    /// <param name="result">The analysis result to render.</param>
    public void RenderResult(AnalysisResult result)
    {
        // Header
        RenderHeader(result);

        // Overview stats
        RenderOverview(result);

        // Commit frequency by day of week
        RenderDayOfWeekChart(result);

        // Commit frequency by hour (heatmap)
        RenderHourlyHeatmap(result);

        // Author contributions
        RenderAuthorTable(result);

        // Most edited files
        RenderMostEditedFiles(result);

        // Streak information
        RenderStreakInfo(result);
    }

    /// <summary>
    /// Renders the header panel.
    /// </summary>
    private static void RenderHeader(AnalysisResult result)
    {
        var header = new FigletText(result.RepositoryName)
            .Color(Color.Blue);

        AnsiConsole.Write(header);

        var panel = new Panel(
            $"[bold]Repository:[/] {result.RepositoryPath}\n" +
            $"[bold]Analysis Period:[/] {result.AnalysisStartDate:yyyy-MM-dd} to {result.AnalysisEndDate:yyyy-MM-dd}")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the overview statistics.
    /// </summary>
    private static void RenderOverview(AnalysisResult result)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Overview[/]").LeftJustified());

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value");

        table.AddRow("Total Commits", $"[green]{result.TotalCommits:N0}[/]");
        table.AddRow("Merge Commits", $"[yellow]{result.MergeCommits:N0}[/]");
        table.AddRow("Lines Added", $"[green]+{result.TotalInsertions:N0}[/]");
        table.AddRow("Lines Deleted", $"[red]-{result.TotalDeletions:N0}[/]");
        table.AddRow("Active Days", $"[cyan]{result.ActiveDays}[/]");
        table.AddRow("Avg Commits/Active Day", $"[blue]{result.AverageCommitsPerActiveDay:F1}[/]");
        table.AddRow("Avg Commits/Week", $"[blue]{result.AverageCommitsPerWeek:F1}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders a bar chart showing commits by day of week.
    /// </summary>
    private static void RenderDayOfWeekChart(AnalysisResult result)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Commits by Day of Week[/]").LeftJustified());

        if (result.CommitsByDayOfWeek.Count == 0 || result.CommitsByDayOfWeek.Values.All(v => v == 0))
        {
            AnsiConsole.MarkupLine("[grey]No commit data available[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var chart = new BarChart()
            .Width(60)
            .Label("[bold]Commits[/]")
            .CenterLabel();

        // Order days starting from Monday for a more natural work-week view
        var orderedDays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        var maxCommits = result.CommitsByDayOfWeek.Values.Max();

        foreach (var day in orderedDays)
        {
            var count = result.CommitsByDayOfWeek.GetValueOrDefault(day, 0);
            var color = GetActivityColor(count, maxCommits);
            chart.AddItem(day.ToString()[..3], count, color);
        }

        AnsiConsole.Write(chart);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders an hourly heatmap of commit activity.
    /// </summary>
    private static void RenderHourlyHeatmap(AnalysisResult result)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Commits by Hour (Heatmap)[/]").LeftJustified());

        if (result.CommitsByHour.Count == 0 || result.CommitsByHour.Values.All(v => v == 0))
        {
            AnsiConsole.MarkupLine("[grey]No commit data available[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var maxCommits = result.CommitsByHour.Values.Max();

        // Create a visual heatmap using colored blocks
        var heatmap = new Table()
            .Border(TableBorder.None)
            .HideHeaders();

        // Add 24 columns for hours
        for (var i = 0; i < 24; i++)
        {
            heatmap.AddColumn(new TableColumn("").Centered());
        }

        // Hour labels row
        var hourLabels = new List<string>();
        for (var hour = 0; hour < 24; hour++)
        {
            hourLabels.Add($"[grey]{hour:D2}[/]");
        }
        heatmap.AddRow(hourLabels.ToArray());

        // Activity blocks row
        var blocks = new List<string>();
        for (var hour = 0; hour < 24; hour++)
        {
            var count = result.CommitsByHour.GetValueOrDefault(hour, 0);
            var color = GetActivityColorName(count, maxCommits);
            blocks.Add($"[{color}]\u2588\u2588[/]");
        }
        heatmap.AddRow(blocks.ToArray());

        // Count labels row
        var countLabels = new List<string>();
        for (var hour = 0; hour < 24; hour++)
        {
            var count = result.CommitsByHour.GetValueOrDefault(hour, 0);
            countLabels.Add(count > 0 ? $"[grey]{count}[/]" : "[grey]·[/]");
        }
        heatmap.AddRow(countLabels.ToArray());

        AnsiConsole.Write(heatmap);

        // Legend
        AnsiConsole.MarkupLine("\n[grey]Legend:[/] [grey30]\u2588[/] None  [green4]\u2588[/] Low  [green]\u2588[/] Medium  [yellow]\u2588[/] High  [red]\u2588[/] Very High");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the author contributions table.
    /// </summary>
    private static void RenderAuthorTable(AnalysisResult result)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Author Contributions[/]").LeftJustified());

        if (result.AuthorContributions.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No author data available[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Author")
            .AddColumn(new TableColumn("Commits").Centered())
            .AddColumn(new TableColumn("%").Centered())
            .AddColumn(new TableColumn("+Lines").RightAligned())
            .AddColumn(new TableColumn("-Lines").RightAligned());

        foreach (var author in result.AuthorContributions.Take(10))
        {
            var percentBar = CreatePercentBar(author.CommitPercentage);
            table.AddRow(
                $"[bold]{Markup.Escape(author.Name)}[/]",
                $"[cyan]{author.CommitCount}[/]",
                $"{percentBar} [grey]{author.CommitPercentage:F1}%[/]",
                $"[green]+{author.TotalInsertions:N0}[/]",
                $"[red]-{author.TotalDeletions:N0}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the most frequently edited files.
    /// </summary>
    private static void RenderMostEditedFiles(AnalysisResult result)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Most Frequently Edited Files (Top 10)[/]").LeftJustified());

        if (result.MostEditedFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No file data available[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("File")
            .AddColumn(new TableColumn("Edits").Centered())
            .AddColumn(new TableColumn("+Lines").RightAligned())
            .AddColumn(new TableColumn("-Lines").RightAligned());

        var maxEdits = result.MostEditedFiles.Max(f => f.EditCount);

        foreach (var file in result.MostEditedFiles)
        {
            var activityBar = CreateActivityBar(file.EditCount, maxEdits);
            table.AddRow(
                $"[blue]{Markup.Escape(TruncatePath(file.FilePath, 50))}[/]",
                $"{activityBar} [cyan]{file.EditCount}[/]",
                $"[green]+{file.TotalInsertions:N0}[/]",
                $"[red]-{file.TotalDeletions:N0}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders streak information.
    /// </summary>
    private static void RenderStreakInfo(AnalysisResult result)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Commit Streak[/]").LeftJustified());

        if (result.LongestCommitStreak <= 0)
        {
            AnsiConsole.MarkupLine("[grey]No streak data available[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var streakPanel = new Panel(
            $"[bold green]{result.LongestCommitStreak}[/] consecutive days\n" +
            (result.LongestStreakStartDate.HasValue && result.LongestStreakEndDate.HasValue
                ? $"[grey]From {result.LongestStreakStartDate:yyyy-MM-dd} to {result.LongestStreakEndDate:yyyy-MM-dd}[/]"
                : ""))
            .Header("[bold]Longest Streak[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);

        AnsiConsole.Write(streakPanel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Gets a color based on activity level.
    /// </summary>
    private static Color GetActivityColor(int count, int maxCount)
    {
        if (count == 0) return Color.Grey30;
        if (maxCount == 0) return Color.Grey30;

        var ratio = (double)count / maxCount;
        return ratio switch
        {
            >= 0.75 => Color.Red,
            >= 0.5 => Color.Yellow,
            >= 0.25 => Color.Green,
            _ => Color.Green4
        };
    }

    /// <summary>
    /// Gets a color name string based on activity level.
    /// </summary>
    private static string GetActivityColorName(int count, int maxCount)
    {
        if (count == 0) return "grey30";
        if (maxCount == 0) return "grey30";

        var ratio = (double)count / maxCount;
        return ratio switch
        {
            >= 0.75 => "red",
            >= 0.5 => "yellow",
            >= 0.25 => "green",
            _ => "green4"
        };
    }

    /// <summary>
    /// Creates a simple percentage bar.
    /// </summary>
    private static string CreatePercentBar(double percentage)
    {
        var filled = (int)(percentage / 10);
        var empty = 10 - filled;
        return $"[green]{"█".PadRight(filled, '█')}[/][grey30]{"░".PadRight(empty, '░')}[/]";
    }

    /// <summary>
    /// Creates an activity bar.
    /// </summary>
    private static string CreateActivityBar(int count, int maxCount)
    {
        var ratio = maxCount > 0 ? (double)count / maxCount : 0;
        var filled = (int)(ratio * 10);
        var empty = 10 - filled;
        return $"[cyan]{"█".PadRight(filled, '█')}[/][grey30]{"░".PadRight(empty, '░')}[/]";
    }

    /// <summary>
    /// Truncates a file path for display.
    /// </summary>
    private static string TruncatePath(string path, int maxLength)
    {
        if (path.Length <= maxLength) return path;
        return "..." + path[^(maxLength - 3)..];
    }
}
