namespace Sorrend.Core.VersionControl;

public class Repository(
    GitVersionControlSystem versionControlSystem,
    string rootDirectoryPath)
{
    public async Task<IReadOnlyList<Commit>> GetFirstParentCommitsAsync()
        => await versionControlSystem.GetFirstParentCommitsAsync(rootDirectoryPath);
}
