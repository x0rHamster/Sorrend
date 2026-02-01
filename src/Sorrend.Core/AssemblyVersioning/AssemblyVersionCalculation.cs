using Sorrend.Core.UserMessages;
using Sorrend.Core.VersionControl;
using Sorrend.Core.VersioningSchemes;
using Sorrend.Core.Versions;

namespace Sorrend.Core.AssemblyVersioning;

public class AssemblyVersionCalculation(IVersioningScheme versioningScheme)
{
    private const int MaximumAssemblyNormalVersionIdentifier = 65534;

    private readonly SemanticVersionIncrement _versionIncrement = new();

    private SemanticVersion? _baseVersion;
    private Commit? _latestIncrementCommit;

    public bool HasResult
        => _baseVersion != null;

    public void Add(Commit commit, AssemblyVersioningConfiguration configuration)
    {
        if (HasResult)
        {
            return;
        }

        var baseVersionFromCommit = versioningScheme.FindBaseVersion(commit, configuration.VersioningSchemes);
        if (baseVersionFromCommit != null)
        {
            using var versionScope = UserMessageScopes.Version(baseVersionFromCommit);

            ValidateAssemblyVersion(baseVersionFromCommit);

            _baseVersion = baseVersionFromCommit;
            _latestIncrementCommit ??= commit;
            return;
        }

        versioningScheme.UpdateVersionIncrement(_versionIncrement, configuration.VersioningSchemes);
        _latestIncrementCommit ??= commit;
    }

    public AssemblyVersionProperties GetResult(AssemblyVersioningConfiguration configuration)
    {
        var baseVersion = _baseVersion ?? versioningScheme.GetInitialVersion(configuration.VersioningSchemes);
        using var baseVersionScope = UserMessageScopes.BaseVersion(baseVersion);

        // TODO increment version should indicate that there were no commits
        var latestIncrementCommit = _latestIncrementCommit ?? throw new NotImplementedException();

        var latestIncrementVersion = versioningScheme.GetIncrementVersion(
            baseVersion,
            _versionIncrement,
            latestIncrementCommit,
            configuration.VersioningSchemes);

        using var versionScope = UserMessageScopes.Version(latestIncrementVersion);

        ValidateAssemblyVersion(latestIncrementVersion);

        var majorVersionsAreIncompatible =
            versioningScheme.AreMajorVersionsIncompatible(configuration.VersioningSchemes);

        return new AssemblyVersionProperties(
            latestIncrementVersion,
            majorVersionsAreIncompatible);
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
