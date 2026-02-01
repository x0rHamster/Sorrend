using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.VersioningSchemes;

public class VersioningSchemeDispatcher(
    SemanticVersioningScheme semanticVersioningScheme,
    CalendarVersioningScheme calendarVersioningScheme)
    : IVersioningScheme
{
    public bool AreMajorVersionsIncompatible(VersioningSchemesConfiguration configuration)
    {
        return GetVersioningScheme(configuration.VersioningScheme)
            .AreMajorVersionsIncompatible(configuration);
    }

    public SemanticVersion GetInitialVersion(VersioningSchemesConfiguration configuration)
    {
        return GetVersioningScheme(configuration.VersioningScheme)
            .GetInitialVersion(configuration);
    }

    public SemanticVersion? FindBaseVersion(Commit commit, VersioningSchemesConfiguration configuration)
    {
        return GetVersioningScheme(configuration.VersioningScheme)
            .FindBaseVersion(commit, configuration);
    }

    public void UpdateVersionIncrement(
        SemanticVersionIncrement versionIncrement,
        VersioningSchemesConfiguration configuration)
    {
        GetVersioningScheme(configuration.VersioningScheme)
            .UpdateVersionIncrement(versionIncrement, configuration);
    }

    public SemanticVersion GetIncrementVersion(
        SemanticVersion baseVersion,
        SemanticVersionIncrement versionIncrement,
        Commit latestIncrementCommit,
        VersioningSchemesConfiguration configuration)
    {
        return GetVersioningScheme(configuration.VersioningScheme)
            .GetIncrementVersion(
                baseVersion,
                versionIncrement,
                latestIncrementCommit,
                configuration);
    }

    private IVersioningScheme GetVersioningScheme(VersioningSchemeIdentifier versioningScheme)
    {
        return versioningScheme switch
        {
            VersioningSchemeIdentifier.SemanticVersioning => semanticVersioningScheme,
            VersioningSchemeIdentifier.CalendarVersioning => calendarVersioningScheme,

            _ => throw new ArgumentOutOfRangeException(
                nameof(versioningScheme),
                versioningScheme,
                null),
        };
    }
}
