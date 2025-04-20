using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Repositories
{
    public class GitRepository
    {
        private const int ShortCommitHashLength = 7;

        private readonly string _rootDirectoryPath;

        public GitRepository(string rootDirectoryPath)
        {
            _rootDirectoryPath = rootDirectoryPath;
        }

        public Task<CommitDescription> CommitAsync(Action<CommitSpecification> configure = null)
        {
            var specification = new CommitSpecification();
            configure?.Invoke(specification);
            return CommitAsync(specification);
        }

        private async Task<CommitDescription> CommitAsync(CommitSpecification specification)
        {
            await RunAsync("git", "add", _rootDirectoryPath);

            var arguments = new List<string>
            {
                "git",
                "commit",
                "--allow-empty",
                "--message=commit",
            };

            if (specification.AuthorDateTime != null)
            {
                arguments.Add($"--date={specification.AuthorDateTime:O}");
            }

            await new SystemCommand()
                .WithWorkingDirectory(_rootDirectoryPath)
                .RunAsync(arguments);

            return await GetHeadCommitAsync();
        }

        public async Task TagAsync(string name)
            => await RunAsync("git", "tag", name);

        public async Task CreateBranchAsync(string name)
            => await RunAsync("git", "branch", name);

        public async Task CheckoutAsync(string revision)
            => await RunAsync("git", "checkout", revision);

        public async Task<CommitDescription> MergeAsync(string revision)
        {
            await RunAsync("git", "merge", "--no-ff", revision);
            return await GetHeadCommitAsync();
        }

        private async Task<CommitDescription> GetHeadCommitAsync()
        {
            var output = await RunAsync("git", "rev-parse", "HEAD");
            var commitHash = output.TrimEnd('\n');
            return new CommitDescription(commitHash, ShortCommitHashLength);
        }

        private async Task<string> RunAsync(params string[] arguments)
        {
            var result = await new SystemCommand()
                .WithWorkingDirectory(_rootDirectoryPath)
                .RunAsync(arguments);

            return result.StandardOutput;
        }
    }
}
