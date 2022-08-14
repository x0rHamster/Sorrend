using System.Collections.Generic;
using System.Threading.Tasks;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Repositories
{
    public class GitRepository
    {
        private readonly string _rootDirectoryPath;

        public GitRepository(string rootDirectoryPath)
        {
            _rootDirectoryPath = rootDirectoryPath;
        }

        public async Task<CommitDescription> CommitAsync()
        {
            await RunAsync("git", "add", _rootDirectoryPath);

            var arguments = new List<string>
            {
                "git",
                "commit",
                "--allow-empty",
                "--message=commit",
            };

            await new SystemCommand()
                .WithWorkingDirectory(_rootDirectoryPath)
                .RunAsync(arguments);

            return await GetHeadCommitAsync();
        }

        private async Task<CommitDescription> GetHeadCommitAsync()
        {
            var output = await RunAsync("git", "rev-parse", "--short", "HEAD");
            var commitShortHash = output.TrimEnd('\n');
            return new CommitDescription(commitShortHash);
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
