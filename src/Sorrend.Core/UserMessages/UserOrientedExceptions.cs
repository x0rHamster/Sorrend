using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JetBrains.Annotations;
using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.UserMessages
{
    internal static class UserOrientedExceptions
    {
        public static Exception CommitHashMustBeHexadecimal(string value)
            => Create(
                "The commit hash {Value} must be a hexadecimal number.",
                value);

        public static Exception TooShortCommitHash(string value, int minimumLength)
            => Create(
                "The commit hash {Value} is shorter than {Limit}.",
                value,
                minimumLength);

        public static Exception TooManyNormalVersionIdentifiers(string normalVersion, int maximumCount)
            => Create(
                "The normal version number {Value} contains more than {Limit} identifiers.",
                normalVersion,
                maximumCount);

        public static Exception NormalVersionIdentifierMustBeNonNegative(string value)
            => Create(
                "The normal version identifier {Value} must be a non-negative integer.",
                value);

        public static Exception NormalVersionIdentifierTooLarge(int value, int maximumValue)
            => Create(
                "The normal version identifier {Value} is greater than {Limit}.",
                value,
                maximumValue);

        public static Exception ZeroNormalVersion()
            => Create(
                "The zero version number \"0.0.0\" is not supported.");

        public static Exception InvalidPreReleaseSuffix(string value)
            => Create(
                "The pre-release suffix {Value} must be a series of dot-separated identifiers preceded by a hyphen.",
                value);

        public static Exception TooManyPreReleaseIdentifiers(string preReleaseSuffix, int maximumCount)
            => Create(
                "The pre-release suffix {Value} contains more than {Limit} identifiers.",
                preReleaseSuffix,
                maximumCount);

        public static Exception NumericPreReleasePrefix(string value)
            => Create(
                "The numeric pre-release prefix {Value} is not supported.",
                value);

        public static Exception PreReleaseCounterMustBeNonNegative(string value)
            => Create(
                "The pre-release counter {Value} must be a non-negative integer.",
                value);

        public static Exception CommitHashPreReleaseIdentifierWithoutPrefix(string value, string prefix)
            => Create(
                "The commit pre-release identifier {Value} does not start with {Prefix}.",
                value,
                prefix);

        public static Exception CommitHashPreReleaseIdentifierDiffersFromCommit(string value, CommitHash commitHash)
            => Create(
                "The commit pre-release identifier {Value} must refer to the commit {CommitHash}.",
                value,
                commitHash);

        public static Exception InvalidBuildMetadataSuffix(string value)
            => Create(
                "The build metadata {Value} must be a series of dot-separated identifiers preceded by a plus sign.",
                value);

        public static Exception InvalidSemanticVersion(string value)
            => Create(
                "{Value} cannot be parsed as a version number.",
                value);

        public static Exception MultipleBaseVersions(IEnumerable<SemanticVersion> versions)
            => Create(
                "Version numbers {Values} cannot be reduced to a single version number.",
                versions);

        public static Exception IncrementOverflowsNormalVersion()
            => Create(
                "Version increments cause the normal version number to overflow. Increase a higher-order normal version identifier.");

        public static Exception IncrementOverflowsPreReleaseCounter()
            => Create(
                "Version increments cause the pre-release counter to overflow. Create a release version.");

        [SuppressMessage(
            "Usage",
            "CA2254:Template should be a static expression",
            Justification = "The caller must provide a constant value that will be forwarded unchanged")]
        private static Exception Create(
            [StructuredMessageTemplate] string messageTemplate,
            params object?[] arguments)
        {
            return UserOrientedExceptionFactory.CreateScoped(
                CultureInfo.InvariantCulture,
                messageTemplate,
                arguments);
        }
    }
}
