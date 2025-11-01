using Sorrend.Core.UserMessages;
using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.AssemblyVersioning;

public class AssemblyVersionCalculation(SemanticVersioningScheme versioningScheme)
{
    private const int MaximumAssemblyNormalVersionIdentifier = 65534;

    private readonly SemanticVersionIncrement _versionIncrement = new();

    private SemanticVersion? _baseVersion;
    private Commit? _latestIncrementCommit;

    public bool HasResult
        => _baseVersion != null;

    public void Add(Commit commit)
    {
        if (HasResult)
        {
            return;
        }

        var baseVersionFromCommit = versioningScheme.FindBaseVersion(commit);
        if (baseVersionFromCommit != null)
        {
            using var versionScope = UserMessageScopes.Version(baseVersionFromCommit);

            ValidateAssemblyVersion(baseVersionFromCommit);

            _baseVersion = baseVersionFromCommit;
            _latestIncrementCommit ??= commit;
            return;
        }

        versioningScheme.UpdateVersionIncrement(_versionIncrement);
        _latestIncrementCommit ??= commit;
    }

    public AssemblyVersionProperties GetResult()
    {
        var baseVersion = _baseVersion ?? versioningScheme.GetInitialVersion();
        using var baseVersionScope = UserMessageScopes.BaseVersion(baseVersion);

        // TODO increment version should indicate that there were no commits
        var latestIncrementCommit = _latestIncrementCommit ?? throw new NotImplementedException();

        var latestIncrementVersion = versioningScheme.GetIncrementVersion(
            baseVersion,
            _versionIncrement,
            latestIncrementCommit);

        using var versionScope = UserMessageScopes.Version(latestIncrementVersion);

        ValidateAssemblyVersion(latestIncrementVersion);

        return new AssemblyVersionProperties(latestIncrementVersion);
    }

    private static void ValidateAssemblyVersion(SemanticVersion version)
    {
        ValidateAssemblyVersionIdentifier(version.MajorVersion);
        ValidateAssemblyVersionIdentifier(version.MinorVersion);
        ValidateAssemblyVersionIdentifier(version.PatchVersion);
    }

    private static void ValidateAssemblyVersionIdentifier(int identifier)
    {
        if (identifier > MaximumAssemblyNormalVersionIdentifier)
        {
            throw UserOrientedExceptions.NormalVersionIdentifierTooLarge(
                identifier,
                MaximumAssemblyNormalVersionIdentifier);
        }
    }
}
