using Sorrend.Core.UserMessages;
using Sorrend.Core.Utilities;
using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.VersioningSchemes;

public class CalendarVersioningScheme(CommitVersionParser commitVersionParser) : IVersioningScheme
{
    private static readonly TimeSpan YearMinutesInterval = TimeSpan.FromMinutes(10);

    public bool AreMajorVersionsIncompatible(VersioningSchemesConfiguration configuration)
        => false;

    public SemanticVersion GetInitialVersion(VersioningSchemesConfiguration configuration)
        => SemanticVersion.Parse("1.0.0-prerelease");

    public SemanticVersion FindBaseVersion(Commit commit, VersioningSchemesConfiguration configuration)
    {
        var candidates = GetBaseVersionCandidates(commit);

        if (candidates.Count > 0)
        {
            var version = GetSingleBaseVersion(candidates);
            ValidateBaseVersion(version);
            ValidateCommitHashEquality(version, commit);
            return version;
        }

        return CalculateBaseVersion(commit.AuthorDateTime);
    }

    private IReadOnlyCollection<SemanticVersion> GetBaseVersionCandidates(Commit commit)
        => commitVersionParser.ParseTags(commit.Tags);

    private static SemanticVersion GetSingleBaseVersion(IReadOnlyCollection<SemanticVersion> candidates)
        => candidates.Count > 1
            ? throw UserOrientedExceptions.MultipleBaseVersions(candidates)
            : candidates.First();

    private static void ValidateBaseVersion(SemanticVersion version)
    {
        if (version.PreReleaseIdentifiers.Count > 1)
        {
            throw UserOrientedExceptions.TooManyPreReleaseIdentifiers(version.PreReleaseSuffix, 1);
        }
    }

    private void ValidateCommitHashEquality(SemanticVersion version, Commit commit)
    {
        if (version.PreReleaseIdentifiers.TryGetValue(index: 0, out var commitHashIdentifier))
        {
            commitVersionParser.ValidateHashPreReleaseIdentifier(commitHashIdentifier, commit.Hash);
        }
    }

    private static SemanticVersion CalculateBaseVersion(DateTimeOffset commitDateTime)
    {
        var commitDateTimeUtc = commitDateTime.ToUniversalTime();
        var timeSinceCommitYear = commitDateTimeUtc - commitDateTimeUtc.GetYearStart();

        var normalVersionIdentifiers = new int[3];
        normalVersionIdentifiers[0] = commitDateTimeUtc.Year;
        normalVersionIdentifiers[1] = (int)(timeSinceCommitYear.Ticks / YearMinutesInterval.Ticks);

        return CreateSemanticVersion(normalVersionIdentifiers, "-prerelease");
    }

    [SuppressMessage(
        "Major Code Smell",
        "S109:Magic numbers should not be used",
        Justification = "This method is too trivial to extract a constant")]
    private static SemanticVersion CreateSemanticVersion(
        IReadOnlyList<int> normalVersionIdentifiers,
        string preReleaseSuffix)
    {
        return new SemanticVersion(
            normalVersionIdentifiers[0],
            normalVersionIdentifiers[1],
            normalVersionIdentifiers[2],
            preReleaseSuffix,
            string.Empty);
    }

    public void UpdateVersionIncrement(
        SemanticVersionIncrement versionIncrement,
        VersioningSchemesConfiguration configuration)
    {
        throw new NotSupportedException(
            "Calendar versioning should not use version increments. Please report a bug.");
    }

    public SemanticVersion GetIncrementVersion(
        SemanticVersion baseVersion,
        SemanticVersionIncrement versionIncrement,
        Commit latestIncrementCommit,
        VersioningSchemesConfiguration configuration)
    {
        ValidateBaseVersion(baseVersion);

        if (!baseVersion.IsPreRelease)
        {
            return new SemanticVersion(
                baseVersion.MajorVersion,
                baseVersion.MinorVersion,
                baseVersion.PatchVersion,
                string.Empty,
                "+" + latestIncrementCommit.Hash);
        }

        var commitHashPreReleaseIdentifier =
            commitVersionParser.GetHashPreReleaseIdentifier(latestIncrementCommit.Hash);

        return new SemanticVersion(
            baseVersion.MajorVersion,
            baseVersion.MinorVersion,
            baseVersion.PatchVersion,
            [commitHashPreReleaseIdentifier],
            string.Empty);
    }
}
