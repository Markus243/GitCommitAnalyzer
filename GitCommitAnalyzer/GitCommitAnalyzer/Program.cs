using GitCommitAnalyzer.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<AnalyzeCommand>();

app.Configure(config =>
{
    config.SetApplicationName("git-analyzer");
    config.SetApplicationVersion("1.0.0");

    config.AddExample([]);
    config.AddExample(["."]);
    config.AddExample(["--days", "30"]);
    config.AddExample(["/path/to/repo", "--days", "60"]);
    config.AddExample(["--output", "stats.json"]);
});

return await app.RunAsync(args);
