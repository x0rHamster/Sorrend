using Sorrend.Core.UserMessages;
using Sorrend.UnitTests.Tools;

namespace Sorrend.UnitTests.Scenarios;

public partial class VersioningTests
{
    [Fact]
    public void UsesPreReleaseCommitHash_AsCommitHashPreReleaseIdentifier()
    {
        var commit = Generate.Commit(hash: "88888888888888888888888888888888");
        var calculation = CreateCalculation();

        calculation.Add(commit);
        var result = calculation.GetResult();

        Assert.EndsWith(".r888888888888", result.Version);
    }

    [Fact]
    public void UsesReleaseCommitHash_AsCommitHashBuildMetadataIdentifier()
    {
        var commit = Generate.Commit(hash: "88888888888888888888888888888888", tag: "2.3.4");
        var calculation = CreateCalculation();

        calculation.Add(commit);
        var result = calculation.GetResult();

        Assert.EndsWith("+88888888888888888888888888888888", result.InformationalVersion);
    }

    [Theory]
    [InlineData("2.3.4-foo.1.r78695a4b3c2d")]
    [InlineData("2.3.4-foo.1.r78695a4b3c2d1e0f0fff0fff0fff0fff")]
    [InlineData("2.3.4-foo.1.r78695a4b3c2d1e0f0fff0fff0fff0fff0fff0fff")]
    [InlineData("2.3.4-foo.1.r7869")]
    public void AcceptsCommitHashPreReleaseIdentifier_WithBaseCommitHash(string commitTag)
    {
        var commit = Generate.Commit(hash: "78695a4b3c2d1e0f0fff0fff0fff0fff0fff0fff", tag: commitTag);
        var calculation = CreateCalculation();

        calculation.Add(commit);
        var result = calculation.GetResult();

        Assert.EndsWith(".r78695a4b3c2d", result.Version);
    }

    [Fact]
    public void RejectsTooShortCommitHashPreReleaseIdentifier()
    {
        var commit = Generate.Commit(hash: "78695a4b3c2d1e0f0fff0fff0fff0fff", tag: "2.3.4-foo.1.r786");
        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit);
                calculation.GetResult();
            });
    }

    [Fact]
    public void AcceptsCommitHashPreReleaseIdentifier_WithDifferentLetterCase()
    {
        var commit = Generate.Commit(
            hash: "0abc1def2abc3def4abc5def6abc7def",
            tag: "2.3.4-foo.1.r0ABC1DEF2ABC3DEF4ABC5DEF6ABC7DEF");

        var calculation = CreateCalculation();

        calculation.Add(commit);
        var result = calculation.GetResult();

        Assert.EndsWith(".r0abc1def2abc", result.Version);
    }

    [Fact]
    public void RejectsCommitHashPreReleaseIdentifier_WithUnknownCommitHash()
    {
        var commit = Generate.Commit(
            hash: "11111111111111111111111111111111",
            tag: "2.3.4-foo.1.r22222222222222222222222222222222");

        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit);
                calculation.GetResult();
            });
    }

    [Theory]
    [InlineData("2.3.4-foo.1.888888888888")]
    [InlineData("2.3.4-foo.1.-888888888888")]
    [InlineData("2.3.4-foo.1.g888888888888")]
    [InlineData("2.3.4-foo.1.R888888888888")]
    public void RejectsInvalidCommitHashPreReleaseIdentifier(string commitTag)
    {
        var commit = Generate.Commit(hash: "88888888888888888888888888888888", tag: commitTag);
        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit);
                calculation.GetResult();
            });
    }
}
