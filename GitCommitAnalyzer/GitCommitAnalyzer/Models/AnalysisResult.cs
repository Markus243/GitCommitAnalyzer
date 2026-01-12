namespace GitCommitAnalyzer.Models;

/// <summary>
/// Contains all calculated statistics from analyzing a Git repository.
/// </summary>
public class AnalysisResult
{
    /// <summary>
    /// The path to the analyzed repository.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// The name of the repository (folder name).
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// The date range start for the analysis.
    /// </summary>
    public DateTime AnalysisStartDate { get; set; }

    /// <summary>
    /// The date range end for the analysis.
    /// </summary>
    public DateTime AnalysisEndDate { get; set; }

    /// <summary>
    /// Total number of commits analyzed.
    /// </summary>
    public int TotalCommits { get; set; }

    /// <summary>
    /// Total number of merge commits.
    /// </summary>
    public int MergeCommits { get; set; }

    /// <summary>
    /// Total lines added across all commits.
    /// </summary>
    public int TotalInsertions { get; set; }

    /// <summary>
    /// Total lines deleted across all commits.
    /// </summary>
    public int TotalDeletions { get; set; }

    /// <summary>
    /// Commit frequency by day of week (0=Sunday, 6=Saturday).
    /// </summary>
    public Dictionary<DayOfWeek, int> CommitsByDayOfWeek { get; set; } = [];

    /// <summary>
    /// Commit frequency by hour of day (0-23).
    /// </summary>
    public Dictionary<int, int> CommitsByHour { get; set; } = [];

    /// <summary>
    /// Most frequently edited files with their edit counts.
    /// </summary>
    public List<FileEditCount> MostEditedFiles { get; set; } = [];

    /// <summary>
    /// Author contribution statistics.
    /// </summary>
    public List<AuthorStats> AuthorContributions { get; set; } = [];

    /// <summary>
    /// Average commits per day (days with at least one commit).
    /// </summary>
    public double AverageCommitsPerActiveDay { get; set; }

    /// <summary>
    /// Average commits per week.
    /// </summary>
    public double AverageCommitsPerWeek { get; set; }

    /// <summary>
    /// The longest streak of consecutive days with commits.
    /// </summary>
    public int LongestCommitStreak { get; set; }

    /// <summary>
    /// The start date of the longest streak.
    /// </summary>
    public DateTime? LongestStreakStartDate { get; set; }

    /// <summary>
    /// The end date of the longest streak.
    /// </summary>
    public DateTime? LongestStreakEndDate { get; set; }

    /// <summary>
    /// Number of unique days with at least one commit.
    /// </summary>
    public int ActiveDays { get; set; }

    /// <summary>
    /// Raw list of commits (for JSON export).
    /// </summary>
    public List<Commit> Commits { get; set; } = [];
}

/// <summary>
/// Statistics for a file's edit frequency.
/// </summary>
public class FileEditCount
{
    /// <summary>
    /// The file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Number of commits that modified this file.
    /// </summary>
    public int EditCount { get; set; }

    /// <summary>
    /// Total lines added to this file.
    /// </summary>
    public int TotalInsertions { get; set; }

    /// <summary>
    /// Total lines deleted from this file.
    /// </summary>
    public int TotalDeletions { get; set; }
}

/// <summary>
/// Statistics for an individual author's contributions.
/// </summary>
public class AuthorStats
{
    /// <summary>
    /// The author's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The author's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Number of commits by this author.
    /// </summary>
    public int CommitCount { get; set; }

    /// <summary>
    /// Total lines added by this author.
    /// </summary>
    public int TotalInsertions { get; set; }

    /// <summary>
    /// Total lines deleted by this author.
    /// </summary>
    public int TotalDeletions { get; set; }

    /// <summary>
    /// Percentage of total commits.
    /// </summary>
    public double CommitPercentage { get; set; }

    /// <summary>
    /// First commit date by this author in the analysis period.
    /// </summary>
    public DateTime FirstCommitDate { get; set; }

    /// <summary>
    /// Last commit date by this author in the analysis period.
    /// </summary>
    public DateTime LastCommitDate { get; set; }
}
