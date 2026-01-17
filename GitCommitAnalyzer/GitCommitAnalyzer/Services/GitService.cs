using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using GitCommitAnalyzer.Models;

namespace GitCommitAnalyzer.Services;

/// <summary>
/// Service for executing Git commands and parsing commit data.
/// </summary>
public partial class GitService
{
    private readonly string _repositoryPath;

    /// <summary>
    /// Initializes a new instance of the GitService.
    /// </summary>
    /// <param name="repositoryPath">Path to the Git repository.</param>
    public GitService(string repositoryPath)
    {
        _repositoryPath = Path.GetFullPath(repositoryPath);
    }

    /// <summary>
    /// Checks if the specified path is a valid Git repository.
    /// </summary>
    /// <returns>True if the path is a Git repository, false otherwise.</returns>
    public async Task<bool> IsGitRepositoryAsync()
    {
        try
        {
            var result = await ExecuteGitCommandAsync("rev-parse --is-inside-work-tree");
            return result.ExitCode == 0 && result.Output.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets all commits within the specified date range.
    /// </summary>
    /// <param name="days">Number of days to look back from today.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>List of parsed commits.</returns>
    public async Task<List<Commit>> GetCommitsAsync(int days, IProgress<string>? progress = null)
    {
        var commits = new List<Commit>();
        var sinceDate = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");

        progress?.Report("Fetching commit log...");

        // Use a custom format for easier parsing
        // Format: hash|full_hash|author|email|date|parents|message
        const string format = "%h|%H|%an|%ae|%aI|%P|%s";
        var logResult = await ExecuteGitCommandAsync($"log --since=\"{sinceDate}\" --format=\"{format}\"");

        if (logResult.ExitCode != 0 || string.IsNullOrWhiteSpace(logResult.Output))
        {
            return commits;
        }

        var lines = logResult.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var totalCommits = lines.Length;
        var processedCommits = 0;

        progress?.Report($"Found {totalCommits} commits. Parsing...");

        foreach (var line in lines)
        {
            var commit = ParseCommitLine(line);
            if (commit != null)
            {
                commits.Add(commit);
            }

            processedCommits++;
            if (processedCommits % 50 == 0 || processedCommits == totalCommits)
            {
                progress?.Report($"Processed {processedCommits}/{totalCommits} commits...");
            }
        }

        // Get file change statistics using numstat
        progress?.Report("Fetching file change statistics...");
        await PopulateFileChangesAsync(commits, sinceDate, progress);

        return commits;
    }

    /// <summary>
    /// Populates file changes for all commits.
    /// </summary>
    private async Task PopulateFileChangesAsync(List<Commit> commits, string sinceDate, IProgress<string>? progress)
    {
        // Get all file changes in one command for efficiency
        var numstatResult = await ExecuteGitCommandAsync($"log --since=\"{sinceDate}\" --format=\"COMMIT:%H\" --numstat");

        if (numstatResult.ExitCode != 0 || string.IsNullOrWhiteSpace(numstatResult.Output))
        {
            return;
        }

        var commitLookup = commits.ToDictionary(c => c.FullHash, c => c);
        Commit? currentCommit = null;

        var lines = numstatResult.Output.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("COMMIT:"))
            {
                var hash = line[7..];
                commitLookup.TryGetValue(hash, out currentCommit);
                continue;
            }

            if (currentCommit == null)
                continue;

            var fileChange = ParseNumstatLine(line);
            if (fileChange != null)
            {
                currentCommit.FileChanges.Add(fileChange);
            }
        }

        progress?.Report("File statistics collected.");
    }

    /// <summary>
    /// Parses a single commit line from the formatted git log output.
    /// </summary>
    /// <param name="line">A pipe-delimited line: hash|full_hash|author|email|date|parents|message</param>
    /// <returns>A parsed Commit object, or null if parsing fails.</returns>
    internal static Commit? ParseCommitLine(string line)
    {
        var parts = line.Split('|');
        if (parts.Length < 7)
            return null;

        var commit = new Commit
        {
            Hash = parts[0],
            FullHash = parts[1],
            Author = parts[2],
            AuthorEmail = parts[3],
            Message = parts[6]
        };

        // Parse ISO 8601 date
        if (DateTime.TryParse(parts[4], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
        {
            commit.Date = date;
        }

        // Check if it's a merge commit (has multiple parents)
        var parents = parts[5];
        commit.IsMergeCommit = parents.Contains(' ');

        return commit;
    }

    /// <summary>
    /// Parses a numstat line (insertions, deletions, filepath).
    /// </summary>
    /// <param name="line">A tab-separated line: insertions deletions filepath</param>
    /// <returns>A parsed FileChange object, or null if parsing fails.</returns>
    internal static FileChange? ParseNumstatLine(string line)
    {
        var match = NumstatRegex().Match(line);
        if (!match.Success)
            return null;

        var insertions = match.Groups["ins"].Value;
        var deletions = match.Groups["del"].Value;
        var filePath = match.Groups["file"].Value;

        var fileChange = new FileChange
        {
            FilePath = filePath
        };

        // Binary files show "-" for insertions/deletions
        if (insertions == "-" || deletions == "-")
        {
            fileChange.IsBinary = true;
        }
        else
        {
            fileChange.Insertions = int.TryParse(insertions, out var ins) ? ins : 0;
            fileChange.Deletions = int.TryParse(deletions, out var del) ? del : 0;
        }

        return fileChange;
    }

    /// <summary>
    /// Executes a Git command and returns the result.
    /// </summary>
    private async Task<(int ExitCode, string Output, string Error)> ExecuteGitCommandAsync(string arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = _repositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (process.ExitCode, output, error);
    }

    /// <summary>
    /// Gets the repository name from the path.
    /// </summary>
    public string GetRepositoryName()
    {
        return Path.GetFileName(_repositoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
               ?? "Unknown Repository";
    }

    [GeneratedRegex(@"^(?<ins>[\d-]+)\s+(?<del>[\d-]+)\s+(?<file>.+)$")]
    private static partial Regex NumstatRegex();
}
