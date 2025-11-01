using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Repositories;

public class GitRepositoryFactory
{
    public async Task<GitRepository> CreateAsync(string directoryPath)
    {
        await new SystemCommand().RunAsync("git", "init", directoryPath);

        return new GitRepository(directoryPath);
    }
}
