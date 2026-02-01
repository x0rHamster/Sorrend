using Sorrend.Core.UserMessages;
using Sorrend.Core.VersioningSchemes;
using Sorrend.UnitTests.Tools;

namespace Sorrend.UnitTests.Scenarios;

public partial class VersioningTests
{
    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, ".r888888888888")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "-r888888888888")]
    public void UsesPreReleaseCommitHash_AsCommitHashPreReleaseIdentifier(
        VersioningSchemeIdentifier versioningScheme,
        string expectedSuffix)
    {
        var commit = Generate.Commit(hash: "88888888888888888888888888888888");
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.EndsWith(expectedSuffix, result.Version);
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning)]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning)]
    public void UsesReleaseCommitHash_AsCommitHashBuildMetadataIdentifier(
        VersioningSchemeIdentifier versioningScheme)
    {
        var commit = Generate.Commit(hash: "88888888888888888888888888888888", tag: "2.3.4");
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.EndsWith("+88888888888888888888888888888888", result.InformationalVersion);
    }

    [Theory]
    [InlineData(
        VersioningSchemeIdentifier.SemanticVersioning,
        "2.3.4-foo.1.r78695a4b3c2d",
        ".r78695a4b3c2d")]
    [InlineData(
        VersioningSchemeIdentifier.CalendarVersioning,
        "2.3.4-r78695a4b3c2d",
        "-r78695a4b3c2d")]
    [InlineData(
        VersioningSchemeIdentifier.SemanticVersioning,
        "2.3.4-foo.1.r78695a4b3c2d1e0f0fff0fff0fff0fff",
        ".r78695a4b3c2d")]
    [InlineData(
        VersioningSchemeIdentifier.CalendarVersioning,
        "2.3.4-r78695a4b3c2d1e0f0fff0fff0fff0fff",
        "-r78695a4b3c2d")]
    [InlineData(
        VersioningSchemeIdentifier.SemanticVersioning,
        "2.3.4-foo.1.r78695a4b3c2d1e0f0fff0fff0fff0fff0fff0fff",
        ".r78695a4b3c2d")]
    [InlineData(
        VersioningSchemeIdentifier.CalendarVersioning,
        "2.3.4-r78695a4b3c2d1e0f0fff0fff0fff0fff0fff0fff",
        "-r78695a4b3c2d")]
    [InlineData(
        VersioningSchemeIdentifier.SemanticVersioning,
        "2.3.4-foo.1.r7869",
        ".r78695a4b3c2d")]
    [InlineData(
        VersioningSchemeIdentifier.CalendarVersioning,
        "2.3.4-r7869",
        "-r78695a4b3c2d")]
    public void AcceptsCommitHashPreReleaseIdentifier_WithBaseCommitHash(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag,
        string expectedSuffix)
    {
        var commit = Generate.Commit(hash: "78695a4b3c2d1e0f0fff0fff0fff0fff0fff0fff", tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.EndsWith(expectedSuffix, result.Version);
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3.4-foo.1.r786")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3.4-r786")]
    public void RejectsTooShortCommitHashPreReleaseIdentifier(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag)
    {
        var commit = Generate.Commit(hash: "78695a4b3c2d1e0f0fff0fff0fff0fff", tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit, configuration);
                calculation.GetResult(configuration);
            });
    }

    [Theory]
    [InlineData(
        VersioningSchemeIdentifier.SemanticVersioning,
        "2.3.4-foo.1.r0ABC1DEF2ABC3DEF4ABC5DEF6ABC7DEF",
        ".r0abc1def2abc")]
    [InlineData(
        VersioningSchemeIdentifier.CalendarVersioning,
        "2.3.4-r0ABC1DEF2ABC3DEF4ABC5DEF6ABC7DEF",
        "-r0abc1def2abc")]
    public void AcceptsCommitHashPreReleaseIdentifier_WithDifferentLetterCase(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag,
        string expectedSuffix)
    {
        var commit = Generate.Commit(hash: "0abc1def2abc3def4abc5def6abc7def", tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.EndsWith(expectedSuffix, result.Version);
    }

    [Theory]
    [InlineData(
        VersioningSchemeIdentifier.SemanticVersioning,
        "2.3.4-foo.1.r22222222222222222222222222222222")]
    [InlineData(
        VersioningSchemeIdentifier.CalendarVersioning,
        "2.3.4-r22222222222222222222222222222222")]
    public void RejectsCommitHashPreReleaseIdentifier_WithUnknownCommitHash(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag)
    {
        var commit = Generate.Commit(hash: "11111111111111111111111111111111", tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit, configuration);
                calculation.GetResult(configuration);
            });
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3.4-foo.1.888888888888")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3.4-888888888888")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3.4-foo.1.-888888888888")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3.4--888888888888")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3.4-foo.1.g888888888888")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3.4-g888888888888")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3.4-foo.1.R888888888888")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3.4-R888888888888")]
    public void RejectsInvalidCommitHashPreReleaseIdentifier(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag)
    {
        var commit = Generate.Commit(hash: "88888888888888888888888888888888", tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit, configuration);
                calculation.GetResult(configuration);
            });
    }
}
