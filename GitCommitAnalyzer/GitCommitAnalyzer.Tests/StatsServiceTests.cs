using GitCommitAnalyzer.Models;
using GitCommitAnalyzer.Services;

namespace GitCommitAnalyzer.Tests;

/// <summary>
/// Unit tests for the StatsService class.
/// </summary>
public class StatsServiceTests
{
    private readonly StatsService _statsService = new();

    #region CalculateStatistics Tests

    [Fact]
    public void CalculateStatistics_EmptyCommitList_ReturnsEmptyResult()
    {
        // Arrange
        var commits = new List<Commit>();

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test/repo", "test-repo", 30);

        // Assert
        Assert.Equal(0, result.TotalCommits);
        Assert.Equal(0, result.MergeCommits);
        Assert.Equal(0, result.TotalInsertions);
        Assert.Equal(0, result.TotalDeletions);
        Assert.Equal("test-repo", result.RepositoryName);
        Assert.Equal("/test/repo", result.RepositoryPath);
    }

    [Fact]
    public void CalculateStatistics_SingleCommit_CalculatesCorrectly()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John Doe", "john@example.com", DateTime.Now, false,
                new List<FileChange>
                {
                    new() { FilePath = "test.cs", Insertions = 10, Deletions = 5 }
                })
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test/repo", "test-repo", 30);

        // Assert
        Assert.Equal(1, result.TotalCommits);
        Assert.Equal(0, result.MergeCommits);
        Assert.Equal(10, result.TotalInsertions);
        Assert.Equal(5, result.TotalDeletions);
        Assert.Equal(1, result.ActiveDays);
        Assert.Equal(1, result.LongestCommitStreak);
    }

    [Fact]
    public void CalculateStatistics_MergeCommits_CountedCorrectly()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John Doe", "john@example.com", DateTime.Now, false),
            CreateCommit("John Doe", "john@example.com", DateTime.Now, true),
            CreateCommit("John Doe", "john@example.com", DateTime.Now, true)
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test/repo", "test-repo", 30);

        // Assert
        Assert.Equal(3, result.TotalCommits);
        Assert.Equal(2, result.MergeCommits);
    }

    [Fact]
    public void CalculateStatistics_MultipleAuthors_CalculatesContributionsCorrectly()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John Doe", "john@example.com", DateTime.Now.AddDays(-1), false,
                new List<FileChange> { new() { FilePath = "a.cs", Insertions = 100, Deletions = 50 } }),
            CreateCommit("John Doe", "john@example.com", DateTime.Now, false,
                new List<FileChange> { new() { FilePath = "b.cs", Insertions = 50, Deletions = 25 } }),
            CreateCommit("Jane Smith", "jane@example.com", DateTime.Now, false,
                new List<FileChange> { new() { FilePath = "c.cs", Insertions = 200, Deletions = 100 } })
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test/repo", "test-repo", 30);

        // Assert
        Assert.Equal(2, result.AuthorContributions.Count);

        var johnStats = result.AuthorContributions.First(a => a.Name == "John Doe");
        Assert.Equal(2, johnStats.CommitCount);
        Assert.Equal(150, johnStats.TotalInsertions);
        Assert.Equal(75, johnStats.TotalDeletions);
        Assert.Equal(2.0 / 3 * 100, johnStats.CommitPercentage, 1); // 2 out of 3 commits = 66.67%

        var janeStats = result.AuthorContributions.First(a => a.Name == "Jane Smith");
        Assert.Equal(1, janeStats.CommitCount);
        Assert.Equal(200, janeStats.TotalInsertions);
        Assert.Equal(100, janeStats.TotalDeletions);
    }

    #endregion

    #region CommitsByDayOfWeek Tests

    [Fact]
    public void CalculateStatistics_CommitsByDayOfWeek_AllDaysInitialized()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13), false) // Monday
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(7, result.CommitsByDayOfWeek.Count);
        Assert.All(Enum.GetValues<DayOfWeek>(), day =>
            Assert.True(result.CommitsByDayOfWeek.ContainsKey(day)));
    }

    [Fact]
    public void CalculateStatistics_CommitsByDayOfWeek_CountsCorrectly()
    {
        // Arrange
        // Jan 12, 2026 = Monday, Jan 13 = Tuesday, Jan 17 = Saturday
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 12), false), // Monday
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 12, 14, 0, 0), false), // Monday
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13), false), // Tuesday
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 17), false), // Saturday
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(2, result.CommitsByDayOfWeek[DayOfWeek.Monday]);
        Assert.Equal(1, result.CommitsByDayOfWeek[DayOfWeek.Tuesday]);
        Assert.Equal(1, result.CommitsByDayOfWeek[DayOfWeek.Saturday]);
        Assert.Equal(0, result.CommitsByDayOfWeek[DayOfWeek.Sunday]);
    }

    #endregion

    #region CommitsByHour Tests

    [Fact]
    public void CalculateStatistics_CommitsByHour_AllHoursInitialized()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13, 10, 30, 0), false)
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(24, result.CommitsByHour.Count);
        for (var hour = 0; hour < 24; hour++)
        {
            Assert.True(result.CommitsByHour.ContainsKey(hour));
        }
    }

    [Fact]
    public void CalculateStatistics_CommitsByHour_CountsCorrectly()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13, 9, 0, 0), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13, 9, 30, 0), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13, 14, 0, 0), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13, 23, 59, 0), false),
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(2, result.CommitsByHour[9]);
        Assert.Equal(1, result.CommitsByHour[14]);
        Assert.Equal(1, result.CommitsByHour[23]);
        Assert.Equal(0, result.CommitsByHour[0]);
    }

    #endregion

    #region MostEditedFiles Tests

    [Fact]
    public void CalculateStatistics_MostEditedFiles_ReturnsTop10()
    {
        // Arrange
        var fileChanges = Enumerable.Range(1, 15)
            .Select(i => new FileChange { FilePath = $"file{i}.cs", Insertions = i, Deletions = 0 })
            .ToList();

        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", DateTime.Now, false, fileChanges)
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(10, result.MostEditedFiles.Count);
    }

    [Fact]
    public void CalculateStatistics_MostEditedFiles_OrderedByEditCount()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", DateTime.Now.AddDays(-2), false,
                new List<FileChange> { new() { FilePath = "a.cs", Insertions = 10, Deletions = 5 } }),
            CreateCommit("John", "john@example.com", DateTime.Now.AddDays(-1), false,
                new List<FileChange>
                {
                    new() { FilePath = "a.cs", Insertions = 20, Deletions = 10 },
                    new() { FilePath = "b.cs", Insertions = 5, Deletions = 2 }
                }),
            CreateCommit("John", "john@example.com", DateTime.Now, false,
                new List<FileChange> { new() { FilePath = "a.cs", Insertions = 5, Deletions = 3 } })
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(2, result.MostEditedFiles.Count);
        Assert.Equal("a.cs", result.MostEditedFiles[0].FilePath);
        Assert.Equal(3, result.MostEditedFiles[0].EditCount);
        Assert.Equal(35, result.MostEditedFiles[0].TotalInsertions);
        Assert.Equal(18, result.MostEditedFiles[0].TotalDeletions);
        Assert.Equal("b.cs", result.MostEditedFiles[1].FilePath);
        Assert.Equal(1, result.MostEditedFiles[1].EditCount);
    }

    #endregion

    #region Streak Tests

    [Fact]
    public void CalculateStatistics_LongestStreak_SingleDay()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15), false)
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(1, result.LongestCommitStreak);
    }

    [Fact]
    public void CalculateStatistics_LongestStreak_ConsecutiveDays()
    {
        // Arrange
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 14), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 16), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 17), false),
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(5, result.LongestCommitStreak);
        Assert.Equal(new DateTime(2026, 1, 13), result.LongestStreakStartDate);
        Assert.Equal(new DateTime(2026, 1, 17), result.LongestStreakEndDate);
    }

    [Fact]
    public void CalculateStatistics_LongestStreak_WithGaps()
    {
        // Arrange
        var commits = new List<Commit>
        {
            // First streak: 2 days
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 10), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 11), false),
            // Gap
            // Second streak: 3 days (longest)
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 16), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 17), false),
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(3, result.LongestCommitStreak);
        Assert.Equal(new DateTime(2026, 1, 15), result.LongestStreakStartDate);
        Assert.Equal(new DateTime(2026, 1, 17), result.LongestStreakEndDate);
    }

    [Fact]
    public void CalculateStatistics_LongestStreak_MultipleCommitsSameDay()
    {
        // Arrange - multiple commits on same day shouldn't break streak
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15, 9, 0, 0), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15, 14, 0, 0), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15, 18, 0, 0), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 16, 10, 0, 0), false),
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(2, result.LongestCommitStreak);
        Assert.Equal(2, result.ActiveDays);
    }

    #endregion

    #region Averages Tests

    [Fact]
    public void CalculateStatistics_AverageCommitsPerActiveDay_CalculatesCorrectly()
    {
        // Arrange - 6 commits over 3 active days
        var commits = new List<Commit>
        {
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 13), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 14), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 14), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15), false),
            CreateCommit("John", "john@example.com", new DateTime(2026, 1, 15), false),
        };

        // Act
        var result = _statsService.CalculateStatistics(commits, "/test", "test", 30);

        // Assert
        Assert.Equal(3, result.ActiveDays);
        Assert.Equal(2.0, result.AverageCommitsPerActiveDay);
    }

    #endregion

    #region Helper Methods

    private static Commit CreateCommit(
        string author,
        string email,
        DateTime date,
        bool isMerge,
        List<FileChange>? fileChanges = null)
    {
        return new Commit
        {
            Hash = Guid.NewGuid().ToString()[..7],
            FullHash = Guid.NewGuid().ToString(),
            Author = author,
            AuthorEmail = email,
            Date = date,
            IsMergeCommit = isMerge,
            Message = "Test commit",
            FileChanges = fileChanges ?? []
        };
    }

    #endregion
}
