using System.Text.RegularExpressions;
using Xunit;

namespace Sorrend.IntegrationTests.Tools;

public static class VersionAssert
{
    public static void NormalVersionEquals(string expected, string actual)
    {
        var parsed = VersionComponents.Parse(actual);
        Assert.Equal(expected, parsed.NormalVersion);
    }

    public static void HasPreRelease(string actual)
    {
        var parsed = VersionComponents.Parse(actual);
        Assert.NotEmpty(parsed.PreReleaseSuffix);
    }

    public static void DoesNotHavePreRelease(string actual)
    {
        var parsed = VersionComponents.Parse(actual);
        Assert.Empty(parsed.PreReleaseSuffix);
    }

    public static void PreReleaseStartsWith(string expected, string actual)
    {
        var parsed = VersionComponents.Parse(actual);
        Assert.NotEmpty(parsed.PreReleaseSuffix);

        Assert.Matches(
            $@"^-{Regex.Escape(expected.Trim('.'))}(?=\.|$)",
            parsed.PreReleaseSuffix);
    }

    public static void PreReleaseEndsWith(string expected, string actual)
    {
        var parsed = VersionComponents.Parse(actual);
        Assert.NotEmpty(parsed.PreReleaseSuffix);

        Assert.Matches(
            $@"(?<=^-|\.){Regex.Escape(expected.Trim('.'))}$",
            parsed.PreReleaseSuffix);
    }

    public static void DoesNotHaveBuildMetadata(string actual)
    {
        var parsed = VersionComponents.Parse(actual);
        Assert.Empty(parsed.BuildMetadataSuffix);
    }

    public static void BuildMetadataEquals(string expected, string actual)
    {
        var parsed = VersionComponents.Parse(actual);
        Assert.Equal("+" + expected, parsed.BuildMetadataSuffix);
    }

    private sealed record VersionComponents(
        string NormalVersion,
        string PreReleaseSuffix,
        string BuildMetadataSuffix)
    {
        private static readonly Regex VersionRegex
            = new(@"^(?<normal>\d+(?:\.\d+)*)(?<prerelease>-[0-9a-zA-Z.\-]+)?(?<build>\+[0-9a-zA-Z.\-]+)?$");

        public static VersionComponents Parse(string version)
        {
            var match = VersionRegex.Match(version);
            Assert.True(match.Success);

            return new VersionComponents(
                match.Groups["normal"].Value,
                match.Groups["prerelease"].Value,
                match.Groups["build"].Value);
        }
    }
}
