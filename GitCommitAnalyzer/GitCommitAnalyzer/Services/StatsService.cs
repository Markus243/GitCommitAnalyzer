using GitCommitAnalyzer.Models;

namespace GitCommitAnalyzer.Services;

/// <summary>
/// Service for calculating statistics from commit data.
/// </summary>
public class StatsService
{
    /// <summary>
    /// Calculates comprehensive statistics from a list of commits.
    /// </summary>
    /// <param name="commits">The commits to analyze.</param>
    /// <param name="repositoryPath">Path to the repository.</param>
    /// <param name="repositoryName">Name of the repository.</param>
    /// <param name="days">Number of days analyzed.</param>
    /// <returns>An AnalysisResult containing all calculated statistics.</returns>
    public AnalysisResult CalculateStatistics(
        List<Commit> commits,
        string repositoryPath,
        string repositoryName,
        int days)
    {
        var result = new AnalysisResult
        {
            RepositoryPath = repositoryPath,
            RepositoryName = repositoryName,
            AnalysisStartDate = DateTime.Now.AddDays(-days),
            AnalysisEndDate = DateTime.Now,
            TotalCommits = commits.Count,
            MergeCommits = commits.Count(c => c.IsMergeCommit),
            TotalInsertions = commits.Sum(c => c.TotalInsertions),
            TotalDeletions = commits.Sum(c => c.TotalDeletions),
            Commits = commits
        };

        if (commits.Count == 0)
        {
            return result;
        }

        // Calculate commit frequency by day of week
        result.CommitsByDayOfWeek = CalculateCommitsByDayOfWeek(commits);

        // Calculate commit frequency by hour
        result.CommitsByHour = CalculateCommitsByHour(commits);

        // Calculate most edited files (top 10)
        result.MostEditedFiles = CalculateMostEditedFiles(commits, 10);

        // Calculate author contributions
        result.AuthorContributions = CalculateAuthorContributions(commits);

        // Calculate averages and streak
        var commitDates = commits.Select(c => c.Date.Date).Distinct().OrderBy(d => d).ToList();
        result.ActiveDays = commitDates.Count;

        if (result.ActiveDays > 0)
        {
            result.AverageCommitsPerActiveDay = (double)commits.Count / result.ActiveDays;
        }

        // Calculate weeks in period
        var totalDays = (result.AnalysisEndDate - result.AnalysisStartDate).TotalDays;
        var weeks = Math.Max(1, totalDays / 7);
        result.AverageCommitsPerWeek = commits.Count / weeks;

        // Calculate longest commit streak
        var (streakLength, streakStart, streakEnd) = CalculateLongestStreak(commitDates);
        result.LongestCommitStreak = streakLength;
        result.LongestStreakStartDate = streakStart;
        result.LongestStreakEndDate = streakEnd;

        return result;
    }

    /// <summary>
    /// Calculates commit distribution by day of week.
    /// </summary>
    private static Dictionary<DayOfWeek, int> CalculateCommitsByDayOfWeek(List<Commit> commits)
    {
        var result = new Dictionary<DayOfWeek, int>();

        // Initialize all days with 0
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            result[day] = 0;
        }

        foreach (var commit in commits)
        {
            result[commit.Date.DayOfWeek]++;
        }

        return result;
    }

    /// <summary>
    /// Calculates commit distribution by hour of day.
    /// </summary>
    private static Dictionary<int, int> CalculateCommitsByHour(List<Commit> commits)
    {
        var result = new Dictionary<int, int>();

        // Initialize all hours with 0
        for (var hour = 0; hour < 24; hour++)
        {
            result[hour] = 0;
        }

        foreach (var commit in commits)
        {
            result[commit.Date.Hour]++;
        }

        return result;
    }

    /// <summary>
    /// Calculates the most frequently edited files.
    /// </summary>
    private static List<FileEditCount> CalculateMostEditedFiles(List<Commit> commits, int topN)
    {
        var fileCounts = new Dictionary<string, FileEditCount>();

        foreach (var commit in commits)
        {
            foreach (var fileChange in commit.FileChanges)
            {
                if (!fileCounts.TryGetValue(fileChange.FilePath, out var count))
                {
                    count = new FileEditCount { FilePath = fileChange.FilePath };
                    fileCounts[fileChange.FilePath] = count;
                }

                count.EditCount++;
                count.TotalInsertions += fileChange.Insertions;
                count.TotalDeletions += fileChange.Deletions;
            }
        }

        return fileCounts.Values
            .OrderByDescending(f => f.EditCount)
            .ThenByDescending(f => f.TotalInsertions + f.TotalDeletions)
            .Take(topN)
            .ToList();
    }

    /// <summary>
    /// Calculates author contribution statistics.
    /// </summary>
    private static List<AuthorStats> CalculateAuthorContributions(List<Commit> commits)
    {
        var authorStats = new Dictionary<string, AuthorStats>();

        foreach (var commit in commits)
        {
            var key = $"{commit.Author}|{commit.AuthorEmail}";

            if (!authorStats.TryGetValue(key, out var stats))
            {
                stats = new AuthorStats
                {
                    Name = commit.Author,
                    Email = commit.AuthorEmail,
                    FirstCommitDate = commit.Date,
                    LastCommitDate = commit.Date
                };
                authorStats[key] = stats;
            }

            stats.CommitCount++;
            stats.TotalInsertions += commit.TotalInsertions;
            stats.TotalDeletions += commit.TotalDeletions;

            if (commit.Date < stats.FirstCommitDate)
                stats.FirstCommitDate = commit.Date;
            if (commit.Date > stats.LastCommitDate)
                stats.LastCommitDate = commit.Date;
        }

        var totalCommits = commits.Count;
        foreach (var stats in authorStats.Values)
        {
            stats.CommitPercentage = totalCommits > 0
                ? (double)stats.CommitCount / totalCommits * 100
                : 0;
        }

        return authorStats.Values
            .OrderByDescending(a => a.CommitCount)
            .ToList();
    }

    /// <summary>
    /// Calculates the longest streak of consecutive commit days.
    /// </summary>
    private static (int Length, DateTime? Start, DateTime? End) CalculateLongestStreak(List<DateTime> sortedDates)
    {
        if (sortedDates.Count == 0)
            return (0, null, null);

        var maxStreak = 1;
        var currentStreak = 1;
        var maxStreakStart = sortedDates[0];
        var maxStreakEnd = sortedDates[0];
        var currentStreakStart = sortedDates[0];

        for (var i = 1; i < sortedDates.Count; i++)
        {
            var daysDiff = (sortedDates[i] - sortedDates[i - 1]).Days;

            if (daysDiff == 1)
            {
                currentStreak++;
            }
            else if (daysDiff > 1)
            {
                if (currentStreak > maxStreak)
                {
                    maxStreak = currentStreak;
                    maxStreakStart = currentStreakStart;
                    maxStreakEnd = sortedDates[i - 1];
                }
                currentStreak = 1;
                currentStreakStart = sortedDates[i];
            }
            // daysDiff == 0 means same day, don't reset streak
        }

        // Check final streak
        if (currentStreak > maxStreak)
        {
            maxStreak = currentStreak;
            maxStreakStart = currentStreakStart;
            maxStreakEnd = sortedDates[^1];
        }

        return (maxStreak, maxStreakStart, maxStreakEnd);
    }
}
