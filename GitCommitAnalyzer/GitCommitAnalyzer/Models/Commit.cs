namespace GitCommitAnalyzer.Models;

/// <summary>
/// Represents a single Git commit with its metadata and file changes.
/// </summary>
public class Commit
{
    /// <summary>
    /// The abbreviated commit hash.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// The full commit hash.
    /// </summary>
    public string FullHash { get; set; } = string.Empty;

    /// <summary>
    /// The name of the commit author.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The email of the commit author.
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the commit was made.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The commit message (first line).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this is a merge commit.
    /// </summary>
    public bool IsMergeCommit { get; set; }

    /// <summary>
    /// List of files changed in this commit.
    /// </summary>
    public List<FileChange> FileChanges { get; set; } = [];

    /// <summary>
    /// Total number of lines added in this commit.
    /// </summary>
    public int TotalInsertions => FileChanges.Sum(f => f.Insertions);

    /// <summary>
    /// Total number of lines deleted in this commit.
    /// </summary>
    public int TotalDeletions => FileChanges.Sum(f => f.Deletions);
}

/// <summary>
/// Represents changes made to a single file in a commit.
/// </summary>
public class FileChange
{
    /// <summary>
    /// The file path relative to repository root.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Number of lines inserted.
    /// </summary>
    public int Insertions { get; set; }

    /// <summary>
    /// Number of lines deleted.
    /// </summary>
    public int Deletions { get; set; }

    /// <summary>
    /// Indicates if the file was binary (stats not available).
    /// </summary>
    public bool IsBinary { get; set; }
}
