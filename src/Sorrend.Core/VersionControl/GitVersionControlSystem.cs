using System.Globalization;
using System.Text.RegularExpressions;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.Core.VersionControl;

public class GitVersionControlSystem
{
    private static readonly Regex LogCommitSeparatorRegex = new(@"(?<=\x00)\x00");
    private static readonly Regex LogCommitPartSeparatorRegex = new(@"\n\x00(?:\n|$)");
    private static readonly Regex LogCommitFieldSeparatorRegex = new(@"\n");
    private static readonly Regex LogCommitTagSeparatorRegex = new("(?:^|, )tag: ");

    public async Task<AbsolutePath> GetRepositoryRootDirectoryPathAsync(AbsolutePath repositoryRelatedDirectoryPath)
    {
        var commandResult = await new SystemCommand()
            .WithWorkingDirectory(repositoryRelatedDirectoryPath)
            .RunAsync("git", "rev-parse", "--show-toplevel");

        return new AbsolutePath(commandResult.StandardOutput.TrimEnd('\n'));
    }

    public async Task<IReadOnlyList<Commit>> GetFirstParentCommitsAsync(AbsolutePath repositoryRootDirectoryPath)
    {
        var logCommandResult = await new SystemCommand()
            .WithWorkingDirectory(repositoryRootDirectoryPath)
            .RunAsync(
                "git",
                "--no-pager",
                "log",
                "--first-parent",
                "--topo-order",
                "--decorate=short",
                "--decorate-refs=refs/tags/",
                "--format=format:%H%n%aI%n%D%n%B%x00",
                "-z",
                "--name-status",
                "--no-renames");

        var commits = new List<Commit>();

        foreach (var logCommit in LogCommitSeparatorRegex.Split(logCommandResult.StandardOutput))
        {
            var logCommitParts = LogCommitPartSeparatorRegex.Split(logCommit, 2);
            var logCommitFields = LogCommitFieldSeparatorRegex.Split(logCommitParts[0], 4);
            var logCommitHash = new CommitHash(logCommitFields[0]);
            var logCommitAuthorDateTime = DateTimeOffset.Parse(logCommitFields[1], CultureInfo.InvariantCulture);
            var logCommitTags = LogCommitTagSeparatorRegex.Split(logCommitFields[2]).Skip(1).ToArray();

            commits.Add(
                new Commit(
                    logCommitHash,
                    logCommitAuthorDateTime,
                    logCommitTags));
        }

        return commits;
    }
}
