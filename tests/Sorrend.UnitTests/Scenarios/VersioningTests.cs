using Sorrend.Core.AssemblyVersioning;
using Sorrend.Core.UserMessages;
using Sorrend.Core.VersioningSchemes;
using Sorrend.UnitTests.Tools;

namespace Sorrend.UnitTests.Scenarios;

public partial class VersioningTests
{
    private static readonly Func<string, string>[] NormalVersionIdentifierPlacements =
    [
        identifier => $"2.3.{identifier}",
        identifier => $"2.{identifier}.4",
        identifier => $"{identifier}.3.4",
    ];

    private static readonly CommitVersionParser DefaultCommitVersionParser = new();

    private static readonly IVersioningScheme DefaultVersioningScheme = new VersioningSchemeDispatcher(
        new SemanticVersioningScheme(DefaultCommitVersionParser),
        new CalendarVersioningScheme(DefaultCommitVersionParser));

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "v2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "v2.3.4")]
    public void UsesBaseVersionFromCommitTag(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag)
    {
        var commit = Generate.Commit(tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.Equal("2.3.4", result.Version);
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "0.0.1")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "0.0.1")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "65534.65534.65534")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "65534.65534.65534")]
    public void SupportsNormalBaseVersions(
        VersioningSchemeIdentifier versioningScheme,
        string version)
    {
        var commit = Generate.Commit(tag: version);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.Equal(version, result.Version);
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "65535")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "65535")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "18446744073709551616")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "18446744073709551616")]
    public void RejectsOverflowingNormalVersionIdentifier(
        VersioningSchemeIdentifier versioningScheme,
        string versionIdentifier)
    {
        foreach (var createVersion in NormalVersionIdentifierPlacements)
        {
            var version = createVersion(versionIdentifier);
            var commit = Generate.Commit(tag: version);
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

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "00")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "00")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "01")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "01")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "-0")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "-0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "-1")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "-1")]
    public void RejectsInvalidNumericNormalVersionIdentifier(
        VersioningSchemeIdentifier versioningScheme,
        string versionIdentifier)
    {
        foreach (var createVersion in NormalVersionIdentifierPlacements)
        {
            var version = createVersion(versionIdentifier);
            var commit = Generate.Commit(tag: version);
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

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "1e1", "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "1e1", "2000.0.0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "1E1", "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "1E1", "2000.0.0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "foo", "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "foo", "2000.0.0")]
    public void IgnoresInvalidNormalVersionIdentifier(
        VersioningSchemeIdentifier versioningScheme,
        string versionIdentifier,
        string expectedVersionPrefix)
    {
        foreach (var createVersion in NormalVersionIdentifierPlacements)
        {
            var version = createVersion(versionIdentifier);
            var rightBaseCommit = Generate.Commit(tag: "2.3.4-foo");
            var wrongBaseCommit = Generate.Commit(date: "2000-01-01Z", tag: version);
            var calculation = CreateCalculation();
            var configuration = CreateConfiguration(versioningScheme);

            calculation.Add(wrongBaseCommit, configuration);
            calculation.Add(rightBaseCommit, configuration);
            var result = calculation.GetResult(configuration);

            Assert.Equal(expectedVersionPrefix, result.VersionPrefix);
        }
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "2.3", "2.3.0")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "2.3", "2.3.0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "v2", "2.0.0")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "v2", "2.0.0")]
    public void SupportsPartialNormalVersions(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag,
        string expectedVersion)
    {
        var commit = Generate.Commit(tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.Equal(expectedVersion, result.Version);
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "0.0.0")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "0.0.0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "0.0")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "0.0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "v0")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "v0")]
    public void RejectsZeroBaseVersion(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag)
    {
        var commit = Generate.Commit(tag: commitTag);
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
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning)]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning)]
    public void IgnoresEarlierBaseVersions(
        VersioningSchemeIdentifier versioningScheme)
    {
        var previousBaseCommit = Generate.Commit(tag: "1.2.3");
        var currentBaseCommit = Generate.Commit(tag: "2.3.4");
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(currentBaseCommit, configuration);
        calculation.Add(previousBaseCommit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.Equal("2.3.4", result.Version);
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "42", "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "42", "2000.0.0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "0", "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "0", "2000.0.0")]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning, "05", "2.3.4")]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning, "05", "2000.0.0")]
    public void DoesNotUseSingleNumberAsBaseVersion(
        VersioningSchemeIdentifier versioningScheme,
        string commitTag,
        string expectedVersionPrefix)
    {
        var baseCommit = Generate.Commit(tag: "2.3.4-foo");
        var incrementCommit = Generate.Commit(date: "2000-01-01Z", tag: commitTag);
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(incrementCommit, configuration);
        calculation.Add(baseCommit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.Equal(expectedVersionPrefix, result.VersionPrefix);
    }

    [Theory]
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning)]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning)]
    public void RejectsExcessNormalVersionIdentifiers(
        VersioningSchemeIdentifier versioningScheme)
    {
        var commit = Generate.Commit(tag: "2.3.4.5");
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
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning)]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning)]
    public void RejectsMultipleBaseVersionsInSingleCommit(
        VersioningSchemeIdentifier versioningScheme)
    {
        var commit = Generate.Commit(tags: ["2.3.4", "3.4.5"]);
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
    [InlineData(VersioningSchemeIdentifier.SemanticVersioning)]
    [InlineData(VersioningSchemeIdentifier.CalendarVersioning)]
    public void IgnoresBuildMetadataInBaseVersion(
        VersioningSchemeIdentifier versioningScheme)
    {
        var commit = Generate.Commit(tag: "2.3.4+foo");
        var calculation = CreateCalculation();
        var configuration = CreateConfiguration(versioningScheme);

        calculation.Add(commit, configuration);
        var result = calculation.GetResult(configuration);

        Assert.DoesNotContain("foo", result.InformationalVersion);
    }

    private static AssemblyVersionCalculation CreateCalculation()
        => new(DefaultVersioningScheme);

    private static AssemblyVersioningConfiguration CreateConfiguration(VersioningSchemeIdentifier versioningScheme)
        => new(new VersioningSchemesConfiguration(versioningScheme));
}
