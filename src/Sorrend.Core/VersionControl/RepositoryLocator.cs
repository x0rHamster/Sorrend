using System.Threading.Tasks;

namespace Sorrend.Core.VersionControl
{
    public class RepositoryLocator(GitVersionControlSystem gitVersionControlSystem)
    {
        public async Task<Repository> GetAsync(string repositoryRelatedDirectoryPath)
        {
            var repositoryRootDirectoryPath = await gitVersionControlSystem
                .GetRepositoryRootDirectoryPathAsync(repositoryRelatedDirectoryPath);

            return new Repository(gitVersionControlSystem, repositoryRootDirectoryPath);
        }
    }
}
