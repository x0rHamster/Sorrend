using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sorrend.Core.VersionControl
{
    public class Repository
    {
        private readonly GitVersionControlSystem _versionControlSystem;
        private readonly string _rootDirectoryPath;

        public Repository(
            GitVersionControlSystem versionControlSystem,
            string rootDirectoryPath)
        {
            _versionControlSystem = versionControlSystem;
            _rootDirectoryPath = rootDirectoryPath;
        }

        public async Task<IReadOnlyList<Commit>> GetFirstParentCommitsAsync()
            => await _versionControlSystem.GetFirstParentCommitsAsync(_rootDirectoryPath);
    }
}
