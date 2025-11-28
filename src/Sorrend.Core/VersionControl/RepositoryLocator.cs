using Sorrend.Core.OperatingSystem;

namespace Sorrend.Core.VersionControl;

public class RepositoryLocator(GitVersionControlSystem gitVersionControlSystem)
{
    public async Task<Repository> GetAsync(AbsolutePath repositoryRelatedDirectoryPath)
    {
        var repositoryRootDirectoryPath = await gitVersionControlSystem
            .GetRepositoryRootDirectoryPathAsync(repositoryRelatedDirectoryPath);

        return new Repository(gitVersionControlSystem, repositoryRootDirectoryPath);
    }
}
