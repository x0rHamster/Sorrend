using Sorrend.Core.AssemblyVersioning;
using Sorrend.Core.UserMessages;
using Sorrend.Core.VersioningSchemes;
using Sorrend.UnitTests.Tools;

namespace Sorrend.UnitTests.Scenarios;

public class SemanticVersioningTests
{
    private static readonly SemanticVersioningScheme DefaultSemanticVersioningScheme = new(
        new CommitVersionParser());

    private static readonly AssemblyVersioningConfiguration DefaultConfiguration = new(
        new VersioningSchemesConfiguration(
            VersioningSchemeIdentifier.SemanticVersioning));

    [Theory]
    [InlineData("2.3.4-dev")]
    [InlineData("2.3.4-foo")]
    [InlineData("2.3.4-foo-bar")]
    [InlineData("2.3.4-1e1")]
    [InlineData("2.3.4-1E1")]
    public void UsesPrefixPreReleaseIdentifierFromBaseVersion(string version)
    {
        var commit = Generate.Commit(tag: version);
        var calculation = CreateCalculation();

        calculation.Add(commit, DefaultConfiguration);
        var result = calculation.GetResult(DefaultConfiguration);

        Assert.StartsWith(version + ".1.", result.Version);
    }

    [Fact]
    public void UsesDefaultPrefixPreReleaseIdentifier()
    {
        var baseCommit = Generate.Commit(tag: "2.3.4");
        var incrementCommit = Generate.Commit();
        var calculation = CreateCalculation();

        calculation.Add(incrementCommit, DefaultConfiguration);
        calculation.Add(baseCommit, DefaultConfiguration);
        var result = calculation.GetResult(DefaultConfiguration);

        Assert.StartsWith("2.3.5-dev.1.", result.Version);
    }

    [Theory]
    [InlineData("2.3.4-foo.1")]
    [InlineData("2.3.4-foo.0")]
    [InlineData("2.3.4-foo.2147483647")]
    public void UsesCounterPreReleaseIdentifierFromBaseVersion(string version)
    {
        var commit = Generate.Commit(tag: version);
        var calculation = CreateCalculation();

        calculation.Add(commit, DefaultConfiguration);
        var result = calculation.GetResult(DefaultConfiguration);

        Assert.StartsWith(version + ".", result.Version);
    }

    [Theory]
    [InlineData("2.3.4-foo.2147483648")]
    [InlineData("2.3.4-foo.18446744073709551616")]
    public void RejectsOverflowingCounterPreReleaseIdentifier(string commitTag)
    {
        var commit = Generate.Commit(tag: commitTag);
        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit, DefaultConfiguration);
                calculation.GetResult(DefaultConfiguration);
            });
    }

    [Fact]
    public void RejectsIncrementCommit_ThatOverflowsCounterPreReleaseIdentifier()
    {
        var baseCommit = Generate.Commit(tag: "2.3.4-foo.2147483647");
        var incrementCommit = Generate.Commit();
        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(incrementCommit, DefaultConfiguration);
                calculation.Add(baseCommit, DefaultConfiguration);
                calculation.GetResult(DefaultConfiguration);
            });
    }

    [Theory]
    [InlineData("2.3.4-foo.00")]
    [InlineData("2.3.4-foo.01")]
    [InlineData("2.3.4-foo.-0")]
    [InlineData("2.3.4-foo.-1")]
    [InlineData("2.3.4-foo.1e1")]
    [InlineData("2.3.4-foo.1E1")]
    public void RejectsInvalidCounterPreReleaseIdentifier(string commitTag)
    {
        var commit = Generate.Commit(tag: commitTag);
        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit, DefaultConfiguration);
                calculation.GetResult(DefaultConfiguration);
            });
    }

    [Fact]
    public void UsesLatestIncrementCommitHash_AsCommitHashPreReleaseIdentifier()
    {
        var baseCommit = Generate.Commit(hash: "11111111111111111111111111111111", tag: "2.3.4");
        var incrementCommit = Generate.Commit(hash: "22222222222222222222222222222222");
        var calculation = CreateCalculation();

        calculation.Add(incrementCommit, DefaultConfiguration);
        calculation.Add(baseCommit, DefaultConfiguration);
        var result = calculation.GetResult(DefaultConfiguration);

        Assert.EndsWith(".r222222222222", result.Version);
    }

    [Fact]
    public void RejectsMultiplePrefixPreReleaseIdentifiers()
    {
        var commit = Generate.Commit(tag: "2.3.4-foo.bar");

        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit, DefaultConfiguration);
                calculation.GetResult(DefaultConfiguration);
            });
    }

    [Theory]
    [InlineData("2.3.4-1")]
    [InlineData("2.3.4-0")]
    [InlineData("2.3.4--0")]
    [InlineData("2.3.4--1")]
    public void RejectsNumericPrefixPreReleaseIdentifier(string commitTag)
    {
        var commit = Generate.Commit(tag: commitTag);

        var calculation = CreateCalculation();

        Assert.ThrowsAny<UserOrientedException>(
            () =>
            {
                calculation.Add(commit, DefaultConfiguration);
                calculation.GetResult(DefaultConfiguration);
            });
    }

    private static AssemblyVersionCalculation CreateCalculation()
        => new(DefaultSemanticVersioningScheme);
}
