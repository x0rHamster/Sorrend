using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Repositories;

public class GitRepositoryFactory
{
    public async Task<GitRepository> CreateAsync(AbsolutePath directoryPath)
    {
        await new SystemCommand().RunAsync("git", "init", directoryPath.ToString());

        return new GitRepository(directoryPath);
    }
}
