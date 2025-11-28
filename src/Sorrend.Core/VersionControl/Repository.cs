using Sorrend.Core.OperatingSystem;

namespace Sorrend.Core.VersionControl;

public class Repository(
    GitVersionControlSystem versionControlSystem,
    AbsolutePath rootDirectoryPath)
{
    public async Task<IReadOnlyList<Commit>> GetFirstParentCommitsAsync()
        => await versionControlSystem.GetFirstParentCommitsAsync(rootDirectoryPath);
}
