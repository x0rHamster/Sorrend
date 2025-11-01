using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Sorrend.Core.Versions
{
    public partial class SemanticVersion
    {
        public int MajorVersion { get; }

        public int MinorVersion { get; }

        public int PatchVersion { get; }

        public string PreReleaseSuffix { get; }

        public IReadOnlyList<string> PreReleaseIdentifiers { get; }

        public string BuildMetadataSuffix { get; }

        public string NormalVersion
            => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";

        public bool IsPreRelease
            => PreReleaseSuffix != string.Empty;

        public SemanticVersion(
            int majorVersion,
            int minorVersion,
            int patchVersion,
            string preReleaseSuffix,
            string buildMetadataSuffix)
            : this(
                majorVersion,
                minorVersion,
                patchVersion,
                preReleaseSuffix,
                SplitPreReleaseSuffix(preReleaseSuffix),
                buildMetadataSuffix)
        {
        }

        public SemanticVersion(
            int majorVersion,
            int minorVersion,
            int patchVersion,
            IReadOnlyList<string> preReleaseIdentifiers,
            string buildMetadataSuffix)
            : this(
                majorVersion,
                minorVersion,
                patchVersion,
                JoinPreReleaseIdentifiers(preReleaseIdentifiers),
                preReleaseIdentifiers,
                buildMetadataSuffix)
        {
        }

        private SemanticVersion(
            int majorVersion,
            int minorVersion,
            int patchVersion,
            string preReleaseSuffix,
            IReadOnlyList<string> preReleaseIdentifiers,
            string buildMetadataSuffix)
        {
            ThrowIfNegative(majorVersion);
            ThrowIfNegative(minorVersion);
            ThrowIfNegative(patchVersion);

            if (majorVersion == 0 && minorVersion == 0 && patchVersion == 0)
            {
                throw new ArgumentException("The zero version number \"0.0.0\" is not supported.");
            }

            if (buildMetadataSuffix != string.Empty)
            {
                ThrowIfDoesNotMatch(StrictBuildMetadataSuffixRegex, buildMetadataSuffix);
            }

            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            PatchVersion = patchVersion;
            PreReleaseSuffix = preReleaseSuffix;
            PreReleaseIdentifiers = preReleaseIdentifiers;
            BuildMetadataSuffix = buildMetadataSuffix;
        }

        private static string[] SplitPreReleaseSuffix(string preReleaseSuffix)
        {
            if (preReleaseSuffix == string.Empty)
            {
                return [];
            }

            ThrowIfDoesNotStartWith("-", StringComparison.Ordinal, preReleaseSuffix);

            return preReleaseSuffix[1..].Split('.');
        }

        [SuppressMessage(
            "Minor Code Smell",
            "S3236:Caller information arguments should not be provided explicitly",
            Justification = "Forces the validation to report the entire argument instead of a part of it")]
        private static string JoinPreReleaseIdentifiers(IReadOnlyList<string> preReleaseIdentifiers)
        {
            if (preReleaseIdentifiers.Count == 0)
            {
                return string.Empty;
            }

            foreach (var preReleaseIdentifier in preReleaseIdentifiers)
            {
                ThrowIfDoesNotMatch(
                    StrictPreReleaseIdentifierRegex,
                    preReleaseIdentifier,
                    nameof(preReleaseIdentifiers));
            }

            return "-" + string.Join(".", preReleaseIdentifiers);
        }

        private static void ThrowIfNegative(
            int argumentValue,
            [CallerArgumentExpression(nameof(argumentValue))]
            string argumentName = "")
        {
            if (argumentValue < 0)
            {
                throw new ArgumentException($"{argumentValue} is less than 0.", argumentName);
            }
        }

        private static void ThrowIfDoesNotStartWith(
            string value,
            StringComparison comparisonType,
            string argumentValue,
            [CallerArgumentExpression(nameof(argumentValue))]
            string argumentName = "")
        {
            if (!argumentValue.StartsWith(value, comparisonType))
            {
                throw new ArgumentException($"\"{argumentValue}\" does not start with \"{value}\".", argumentName);
            }
        }

        private static void ThrowIfDoesNotMatch(
            Regex regex,
            string argumentValue,
            [CallerArgumentExpression(nameof(argumentValue))]
            string argumentName = "")
        {
            if (!regex.IsMatch(argumentValue))
            {
                throw new ArgumentException($"\"{argumentValue}\" does not match regex \"{regex}\".", argumentName);
            }
        }

        public override string ToString()
            => NormalVersion + PreReleaseSuffix + BuildMetadataSuffix;
    }
}
