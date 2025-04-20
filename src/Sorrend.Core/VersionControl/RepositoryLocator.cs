using System.Threading.Tasks;

namespace Sorrend.Core.VersionControl
{
    public class RepositoryLocator
    {
        private readonly GitVersionControlSystem _gitVersionControlSystem;

        public RepositoryLocator(GitVersionControlSystem gitVersionControlSystem)
        {
            _gitVersionControlSystem = gitVersionControlSystem;
        }

        public async Task<Repository> GetAsync(string repositoryRelatedDirectoryPath)
        {
            var repositoryRootDirectoryPath = await _gitVersionControlSystem
                .GetRepositoryRootDirectoryPathAsync(repositoryRelatedDirectoryPath);

            return new Repository(_gitVersionControlSystem, repositoryRootDirectoryPath);
        }
    }
}
