using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.VersioningSchemes;

public interface IVersioningScheme
{
    bool AreMajorVersionsIncompatible(VersioningSchemesConfiguration configuration);

    SemanticVersion GetInitialVersion(VersioningSchemesConfiguration configuration);

    SemanticVersion? FindBaseVersion(Commit commit, VersioningSchemesConfiguration configuration);

    void UpdateVersionIncrement(
        SemanticVersionIncrement versionIncrement,
        VersioningSchemesConfiguration configuration);

    SemanticVersion GetIncrementVersion(
        SemanticVersion baseVersion,
        SemanticVersionIncrement versionIncrement,
        Commit latestIncrementCommit,
        VersioningSchemesConfiguration configuration);
}
