using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Sorrend.Core.Utilities;

namespace Sorrend.Core.Versions
{
    public partial class SemanticVersion
    {
        private const int MaximumNormalVersionIdentifierCount = 3;

        private static readonly Regex StrictPreReleaseIdentifierRegex
            = new Regex(@"(?:0|[1-9]\d*|\d*[a-zA-Z\-][0-9a-zA-Z\-]*)");

        private static readonly Regex StrictBuildMetadataIdentifierRegex
            = new Regex(@"[0-9a-zA-Z\-]+");

        private static readonly Regex StrictPreReleaseSuffixRegex
            = @"^-{0}(?:\.{0})*$".FormatRegex(StrictPreReleaseIdentifierRegex);

        private static readonly Regex StrictBuildMetadataSuffixRegex
            = @"^\+{0}(?:\.{0})*$".FormatRegex(StrictBuildMetadataIdentifierRegex);

        private static readonly Regex LooseVersionRegex
            = "^(?<normal>{0})(?<prerelease>{1})?(?<build>{2})?$".FormatRegex(
                @"-?\d+(?:\.-?\d+)*",
                @"-[0-9a-zA-Z.\-]+",
                @"\+[0-9a-zA-Z.\-]+");

        [CanBeNull]
        public static SemanticVersion ParseOrDefault(string version)
        {
            var match = LooseVersionRegex.Match(version);
            if (!match.Success)
            {
                return null;
            }

            var normalVersion = match.Groups["normal"].Value;
            var normalVersionIdentifiers = normalVersion.Split('.');
            if (normalVersionIdentifiers.Length > MaximumNormalVersionIdentifierCount)
            {
                throw new UserOrientedException(
                    $"The normal version number \"{normalVersion}\" contains more than {MaximumNormalVersionIdentifierCount} identifiers.");
            }

            var majorVersion = ParseNormalVersionIdentifier(normalVersionIdentifiers, 0);
            var minorVersion = ParseNormalVersionIdentifier(normalVersionIdentifiers, 1);
            var patchVersion = ParseNormalVersionIdentifier(normalVersionIdentifiers, 2);

            if (majorVersion == 0 && minorVersion == 0 && patchVersion == 0)
            {
                throw new UserOrientedException("The zero version number \"0.0.0\" is not supported.");
            }

            var preReleaseSuffix = match.Groups["prerelease"].Value;
            if (preReleaseSuffix != string.Empty && !StrictPreReleaseSuffixRegex.IsMatch(preReleaseSuffix))
            {
                throw new UserOrientedException(
                    $"The pre-release suffix \"{preReleaseSuffix}\" must be a series of dot-separated identifiers preceded by a hyphen.");
            }

            var buildMetadataSuffix = match.Groups["build"].Value;
            if (buildMetadataSuffix != string.Empty && !StrictBuildMetadataSuffixRegex.IsMatch(buildMetadataSuffix))
            {
                throw new UserOrientedException(
                    $"The build metadata \"{buildMetadataSuffix}\" must be a series of dot-separated identifiers preceded by a plus sign.");
            }

            return new SemanticVersion(
                majorVersion,
                minorVersion,
                patchVersion,
                preReleaseSuffix,
                buildMetadataSuffix);
        }

        public static SemanticVersion Parse(string version)
            => ParseOrDefault(version)
                ?? throw new UserOrientedException($"\"{version}\" cannot be parsed as a version number.");

        private static int ParseNormalVersionIdentifier(string[] identifiers, int identifierIndex)
        {
            if (identifierIndex > identifiers.GetUpperBound(0))
            {
                return 0;
            }

            var identifier = identifiers[identifierIndex];

            if (!identifier.TryParseCanonicalInteger(out var value) || value < 0)
            {
                throw new UserOrientedException(
                    $"The normal version identifier \"{identifier}\" must be a non-negative integer.");
            }

            return value;
        }
    }
}
