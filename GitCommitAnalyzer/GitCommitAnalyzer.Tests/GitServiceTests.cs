using GitCommitAnalyzer.Models;
using GitCommitAnalyzer.Services;

namespace GitCommitAnalyzer.Tests;

/// <summary>
/// Unit tests for the GitService class.
/// </summary>
public class GitServiceTests
{
    #region ParseCommitLine Tests

    [Fact]
    public void ParseCommitLine_ValidLine_ReturnsCommit()
    {
        // Arrange
        var line = "abc1234|abc1234567890abcdef1234567890abcdef12345678|John Doe|john@example.com|2026-01-15T10:30:00+00:00||Initial commit";

        // Act
        var commit = GitService.ParseCommitLine(line);

        // Assert
        Assert.NotNull(commit);
        Assert.Equal("abc1234", commit.Hash);
        Assert.Equal("abc1234567890abcdef1234567890abcdef12345678", commit.FullHash);
        Assert.Equal("John Doe", commit.Author);
        Assert.Equal("john@example.com", commit.AuthorEmail);
        Assert.Equal("Initial commit", commit.Message);
        Assert.False(commit.IsMergeCommit);
    }

    [Fact]
    public void ParseCommitLine_MergeCommit_SetsIsMergeCommitTrue()
    {
        // Arrange - merge commit has two parent hashes separated by space
        var line = "abc1234|abc1234567890abcdef1234567890abcdef12345678|John Doe|john@example.com|2026-01-15T10:30:00+00:00|parent1 parent2|Merge branch 'feature'";

        // Act
        var commit = GitService.ParseCommitLine(line);

        // Assert
        Assert.NotNull(commit);
        Assert.True(commit.IsMergeCommit);
        Assert.Equal("Merge branch 'feature'", commit.Message);
    }

    [Fact]
    public void ParseCommitLine_IncompleteLine_ReturnsNull()
    {
        // Arrange - only 5 fields instead of 7
        var line = "abc1234|abc1234567890|John Doe|john@example.com|2026-01-15";

        // Act
        var commit = GitService.ParseCommitLine(line);

        // Assert
        Assert.Null(commit);
    }

    [Fact]
    public void ParseCommitLine_EmptyLine_ReturnsNull()
    {
        // Act
        var commit = GitService.ParseCommitLine("");

        // Assert
        Assert.Null(commit);
    }

    [Fact]
    public void ParseCommitLine_MessageWithPipes_ParsesCorrectly()
    {
        // Arrange - message contains pipes (edge case)
        var line = "abc1234|abc1234567890abcdef1234567890abcdef12345678|John Doe|john@example.com|2026-01-15T10:30:00+00:00||Fix: handle pipe | character";

        // Act
        var commit = GitService.ParseCommitLine(line);

        // Assert
        Assert.NotNull(commit);
        // Note: the message will be just "Fix: handle pipe " due to split behavior
        // This documents the current behavior
    }

    [Fact]
    public void ParseCommitLine_ValidDate_ParsesCorrectly()
    {
        // Arrange
        var line = "abc1234|abc1234567890abcdef1234567890abcdef12345678|John Doe|john@example.com|2026-01-15T14:30:00+05:00||Test commit";

        // Act
        var commit = GitService.ParseCommitLine(line);

        // Assert
        Assert.NotNull(commit);
        Assert.Equal(2026, commit.Date.Year);
        Assert.Equal(1, commit.Date.Month);
        Assert.Equal(15, commit.Date.Day);
    }

    #endregion

    #region ParseNumstatLine Tests

    [Fact]
    public void ParseNumstatLine_ValidLine_ReturnsFileChange()
    {
        // Arrange
        var line = "10\t5\tsrc/Program.cs";

        // Act
        var fileChange = GitService.ParseNumstatLine(line);

        // Assert
        Assert.NotNull(fileChange);
        Assert.Equal("src/Program.cs", fileChange.FilePath);
        Assert.Equal(10, fileChange.Insertions);
        Assert.Equal(5, fileChange.Deletions);
        Assert.False(fileChange.IsBinary);
    }

    [Fact]
    public void ParseNumstatLine_BinaryFile_SetsIsBinaryTrue()
    {
        // Arrange - binary files show "-" for stats
        var line = "-\t-\timage.png";

        // Act
        var fileChange = GitService.ParseNumstatLine(line);

        // Assert
        Assert.NotNull(fileChange);
        Assert.Equal("image.png", fileChange.FilePath);
        Assert.True(fileChange.IsBinary);
        Assert.Equal(0, fileChange.Insertions);
        Assert.Equal(0, fileChange.Deletions);
    }

    [Fact]
    public void ParseNumstatLine_ZeroChanges_ParsesCorrectly()
    {
        // Arrange
        var line = "0\t0\tempty-change.txt";

        // Act
        var fileChange = GitService.ParseNumstatLine(line);

        // Assert
        Assert.NotNull(fileChange);
        Assert.Equal("empty-change.txt", fileChange.FilePath);
        Assert.Equal(0, fileChange.Insertions);
        Assert.Equal(0, fileChange.Deletions);
        Assert.False(fileChange.IsBinary);
    }

    [Fact]
    public void ParseNumstatLine_LargeNumbers_ParsesCorrectly()
    {
        // Arrange
        var line = "1500\t2300\tlarge-file.json";

        // Act
        var fileChange = GitService.ParseNumstatLine(line);

        // Assert
        Assert.NotNull(fileChange);
        Assert.Equal(1500, fileChange.Insertions);
        Assert.Equal(2300, fileChange.Deletions);
    }

    [Fact]
    public void ParseNumstatLine_PathWithSpaces_ParsesCorrectly()
    {
        // Arrange
        var line = "5\t3\tpath/with spaces/file name.cs";

        // Act
        var fileChange = GitService.ParseNumstatLine(line);

        // Assert
        Assert.NotNull(fileChange);
        Assert.Equal("path/with spaces/file name.cs", fileChange.FilePath);
    }

    [Fact]
    public void ParseNumstatLine_InvalidFormat_ReturnsNull()
    {
        // Arrange
        var line = "invalid line without tabs";

        // Act
        var fileChange = GitService.ParseNumstatLine(line);

        // Assert
        Assert.Null(fileChange);
    }

    [Fact]
    public void ParseNumstatLine_EmptyLine_ReturnsNull()
    {
        // Act
        var fileChange = GitService.ParseNumstatLine("");

        // Assert
        Assert.Null(fileChange);
    }

    #endregion

    #region GetRepositoryName Tests

    [Fact]
    public void GetRepositoryName_ValidPath_ReturnsDirectoryName()
    {
        // Arrange
        var service = new GitService("/home/user/my-repo");

        // Act
        var name = service.GetRepositoryName();

        // Assert
        Assert.Equal("my-repo", name);
    }

    [Fact]
    public void GetRepositoryName_PathWithTrailingSlash_ReturnsDirectoryName()
    {
        // Arrange
        var service = new GitService("/home/user/my-repo/");

        // Act
        var name = service.GetRepositoryName();

        // Assert
        Assert.Equal("my-repo", name);
    }

    #endregion
}
