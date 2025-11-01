using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sorrend.Core.UserMessages;
using Sorrend.Core.Utilities;
using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.AssemblyVersioning
{
    public class SemanticVersioningScheme(CommitVersionParser commitVersionParser)
    {
        private const int MaximumPreReleaseIdentifierCount = 3;

        private const string InitialVersion = "0.1.0-dev.0";
        private const string DefaultPrefixPreReleaseIdentifier = "dev";

        public SemanticVersion GetInitialVersion()
            => SemanticVersion.Parse(InitialVersion);

        public SemanticVersion? FindBaseVersion(Commit commit)
        {
            var candidates = GetBaseVersionCandidates(commit);

            if (candidates.Count == 0)
            {
                return null;
            }

            var version = GetSingleBaseVersion(candidates);
            ValidateBaseVersion(version);
            ValidateCommitHashEquality(version, commit);
            return version;
        }

        private IReadOnlyCollection<SemanticVersion> GetBaseVersionCandidates(Commit commit)
            => commitVersionParser.ParseTags(commit.Tags);

        private static SemanticVersion GetSingleBaseVersion(IReadOnlyCollection<SemanticVersion> candidates)
            => candidates.Count > 1
                ? throw UserOrientedExceptions.MultipleBaseVersions(candidates)
                : candidates.First();

        private static void ValidateBaseVersion(SemanticVersion version)
        {
            if (
                version.PreReleaseIdentifiers.TryGetValue(index: 0, out var prefixIdentifier)
                && prefixIdentifier.IsInteger())
            {
                throw UserOrientedExceptions.NumericPreReleasePrefix(prefixIdentifier);
            }

            if (
                version.PreReleaseIdentifiers.TryGetValue(index: 1, out var counterIdentifier)
                && (
                    !counterIdentifier.TryParseCanonicalInteger(out var counterIdentifierValue)
                    || counterIdentifierValue < 0))
            {
                throw UserOrientedExceptions.PreReleaseCounterMustBeNonNegative(counterIdentifier);
            }

            if (version.PreReleaseIdentifiers.Count > MaximumPreReleaseIdentifierCount)
            {
                throw UserOrientedExceptions.TooManyPreReleaseIdentifiers(
                    version.PreReleaseSuffix,
                    MaximumPreReleaseIdentifierCount);
            }
        }

        private void ValidateCommitHashEquality(SemanticVersion version, Commit commit)
        {
            if (version.PreReleaseIdentifiers.TryGetValue(index: 2, out var commitHashIdentifier))
            {
                commitVersionParser.ValidateHashPreReleaseIdentifier(commitHashIdentifier, commit.Hash);
            }
        }

        public void UpdateVersionIncrement(SemanticVersionIncrement versionIncrement)
        {
            versionIncrement.AddPreRelease(SemanticVersioningReleaseType.Patch);
        }

        public SemanticVersion GetIncrementVersion(
            SemanticVersion baseVersion,
            SemanticVersionIncrement versionIncrement,
            Commit latestIncrementCommit)
        {
            ValidateBaseVersion(baseVersion);

            var incrementMajorVersion = versionIncrement.GetMajorVersion(baseVersion);
            var incrementMinorVersion = versionIncrement.GetMinorVersion(baseVersion);
            var incrementPatchVersion = versionIncrement.GetPatchVersion(baseVersion);

            var incrementCommitHash = latestIncrementCommit.Hash;

            var basePrefixPreReleaseIdentifier = baseVersion.PreReleaseIdentifiers
                .GetValueOrDefault(index: 0);

            var incrementPrefixPreReleaseIdentifier = versionIncrement.GetPrefixPreReleaseIdentifier(
                baseVersion,
                basePrefixPreReleaseIdentifier,
                DefaultPrefixPreReleaseIdentifier);

            if (incrementPrefixPreReleaseIdentifier == null)
            {
                return new SemanticVersion(
                    incrementMajorVersion,
                    incrementMinorVersion,
                    incrementPatchVersion,
                    string.Empty,
                    "+" + incrementCommitHash);
            }

            var baseCounterPreReleaseIdentifier = baseVersion.PreReleaseIdentifiers
                .TryGetValue(index: 1, out var baseCounterPreReleaseIdentifierAsString)
                ? int.Parse(baseCounterPreReleaseIdentifierAsString, CultureInfo.InvariantCulture)
                : (int?)null;

            var incrementCounterPreReleaseIdentifier = versionIncrement
                    .GetCounterPreReleaseIdentifier(baseVersion, baseCounterPreReleaseIdentifier)
                ?? throw new InvalidOperationException(
                    "The version increment returned a pre-release prefix, but cannot return a pre-release counter. Please report a bug.");

            var incrementCommitHashPreReleaseIdentifier =
                commitVersionParser.GetHashPreReleaseIdentifier(incrementCommitHash);

            return new SemanticVersion(
                incrementMajorVersion,
                incrementMinorVersion,
                incrementPatchVersion,
                [
                    incrementPrefixPreReleaseIdentifier,
                    incrementCounterPreReleaseIdentifier.ToString(CultureInfo.InvariantCulture),
                    incrementCommitHashPreReleaseIdentifier,
                ],
                string.Empty);
        }
    }
}
