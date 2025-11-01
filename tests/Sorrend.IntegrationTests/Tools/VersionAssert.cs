using System.Text.RegularExpressions;

namespace Sorrend.IntegrationTests.Tools;

public static class VersionAssert
{
    private static readonly Regex VersionRegex
        = new(@"^(?<normal>\d+(?:\.\d+)*)(?<prerelease>-[0-9a-zA-Z.\-]+)?(?<build>\+[0-9a-zA-Z.\-]+)?$");

    public static void NormalVersionEquals(string expected, string actual)
    {
        var match = VersionRegex.Match(actual);
        Assert.True(match.Success);
        Assert.Equal(expected, match.Groups["normal"].Value);
    }

    public static void HasPreRelease(string actual)
    {
        var match = VersionRegex.Match(actual);
        Assert.True(match.Success);
        Assert.True(match.Groups["prerelease"].Success);
    }

    public static void DoesNotHavePreRelease(string actual)
    {
        var match = VersionRegex.Match(actual);
        Assert.True(match.Success);
        Assert.False(match.Groups["prerelease"].Success);
    }

    public static void PreReleaseStartsWith(string expected, string actual)
    {
        var match = VersionRegex.Match(actual);
        Assert.True(match.Success);
        Assert.True(match.Groups["prerelease"].Success);

        Assert.Matches(
            $@"^-{Regex.Escape(expected.Trim('.'))}(?=\.|$)",
            match.Groups["prerelease"].Value);
    }

    public static void PreReleaseEndsWith(string expected, string actual)
    {
        var match = VersionRegex.Match(actual);
        Assert.True(match.Success);
        Assert.True(match.Groups["prerelease"].Success);

        Assert.Matches(
            $@"(?<=^-|\.){Regex.Escape(expected.Trim('.'))}$",
            match.Groups["prerelease"].Value);
    }

    public static void DoesNotHaveBuildMetadata(string actual)
    {
        var match = VersionRegex.Match(actual);
        Assert.True(match.Success);
        Assert.False(match.Groups["build"].Success);
    }

    public static void BuildMetadataEquals(string expected, string actual)
    {
        var match = VersionRegex.Match(actual);
        Assert.True(match.Success);
        Assert.True(match.Groups["build"].Success);
        Assert.Equal("+" + expected, match.Groups["build"].Value);
    }
}
