using System;
using Sorrend.Core;
using Sorrend.Core.AssemblyVersioning;
using Sorrend.UnitTests.Tools;
using Xunit;

namespace Sorrend.UnitTests.Scenarios
{
    public partial class VersioningTests
    {
        private static readonly Func<string, string>[] NormalVersionIdentifierPlacements =
        {
            identifier => $"2.3.{identifier}",
            identifier => $"2.{identifier}.4",
            identifier => $"{identifier}.3.4",
        };

        private static readonly SemanticVersioningScheme DefaultVersioningScheme
            = new SemanticVersioningScheme(new CommitVersionParser());

        [Theory]
        [InlineData("2.3.4")]
        [InlineData("v2.3.4")]
        public void UsesBaseVersionFromCommitTag(string commitTag)
        {
            var commit = Generate.Commit(tag: commitTag);
            var calculation = CreateCalculation();

            calculation.Add(commit);
            var result = calculation.GetResult();

            Assert.Equal("2.3.4", result.Version);
        }

        [Theory]
        [InlineData("2.3.4")]
        [InlineData("0.0.1")]
        [InlineData("65534.65534.65534")]
        public void SupportsNormalBaseVersions(string version)
        {
            var commit = Generate.Commit(tag: version);
            var calculation = CreateCalculation();

            calculation.Add(commit);
            var result = calculation.GetResult();

            Assert.Equal(version, result.Version);
        }

        [Theory]
        [InlineData("65535")]
        [InlineData("18446744073709551616")]
        public void RejectsOverflowingNormalVersionIdentifier(string versionIdentifier)
        {
            foreach (var createVersion in NormalVersionIdentifierPlacements)
            {
                var version = createVersion(versionIdentifier);
                var commit = Generate.Commit(tag: version);
                var calculation = CreateCalculation();

                Assert.ThrowsAny<UserOrientedException>(
                    () =>
                    {
                        calculation.Add(commit);
                        calculation.GetResult();
                    });
            }
        }

        [Theory]
        [InlineData("00")]
        [InlineData("01")]
        [InlineData("-0")]
        [InlineData("-1")]
        public void RejectsInvalidNumericNormalVersionIdentifier(string versionIdentifier)
        {
            foreach (var createVersion in NormalVersionIdentifierPlacements)
            {
                var version = createVersion(versionIdentifier);
                var commit = Generate.Commit(tag: version);
                var calculation = CreateCalculation();

                Assert.ThrowsAny<UserOrientedException>(
                    () =>
                    {
                        calculation.Add(commit);
                        calculation.GetResult();
                    });
            }
        }

        [Theory]
        [InlineData("1e1", "2.3.4")]
        [InlineData("1E1", "2.3.4")]
        [InlineData("foo", "2.3.4")]
        public void IgnoresInvalidNormalVersionIdentifier(
            string versionIdentifier,
            string expectedVersionPrefix)
        {
            foreach (var createVersion in NormalVersionIdentifierPlacements)
            {
                var version = createVersion(versionIdentifier);
                var rightBaseCommit = Generate.Commit(tag: "2.3.4-foo");
                var wrongBaseCommit = Generate.Commit(date: "2000-01-01Z", tag: version);
                var calculation = CreateCalculation();

                calculation.Add(wrongBaseCommit);
                calculation.Add(rightBaseCommit);
                var result = calculation.GetResult();

                Assert.Equal(expectedVersionPrefix, result.VersionPrefix);
            }
        }

        [Theory]
        [InlineData("2.3", "2.3.0")]
        [InlineData("v2", "2.0.0")]
        public void SupportsPartialNormalVersions(
            string commitTag,
            string expectedVersion)
        {
            var commit = Generate.Commit(tag: commitTag);
            var calculation = CreateCalculation();

            calculation.Add(commit);
            var result = calculation.GetResult();

            Assert.Equal(expectedVersion, result.Version);
        }

        [Theory]
        [InlineData("0.0.0")]
        [InlineData("0.0")]
        [InlineData("v0")]
        public void RejectsZeroBaseVersion(string commitTag)
        {
            var commit = Generate.Commit(tag: commitTag);
            var calculation = CreateCalculation();

            Assert.ThrowsAny<UserOrientedException>(
                () =>
                {
                    calculation.Add(commit);
                    calculation.GetResult();
                });
        }

        [Fact]
        public void IgnoresEarlierBaseVersions()
        {
            var previousBaseCommit = Generate.Commit(tag: "1.2.3");
            var currentBaseCommit = Generate.Commit(tag: "2.3.4");
            var calculation = CreateCalculation();

            calculation.Add(currentBaseCommit);
            calculation.Add(previousBaseCommit);
            var result = calculation.GetResult();

            Assert.Equal("2.3.4", result.Version);
        }

        [Theory]
        [InlineData("42")]
        [InlineData("0")]
        [InlineData("05")]
        public void DoesNotUseSingleNumberAsBaseVersion(string commitTag)
        {
            var baseCommit = Generate.Commit(tag: "2.3.4-foo");
            var incrementCommit = Generate.Commit(date: "2000-01-01Z", tag: commitTag);
            var calculation = CreateCalculation();

            calculation.Add(incrementCommit);
            calculation.Add(baseCommit);
            var result = calculation.GetResult();

            Assert.Equal("2.3.4", result.VersionPrefix);
        }

        [Fact]
        public void RejectsExcessNormalVersionIdentifiers()
        {
            var commit = Generate.Commit(tag: "2.3.4.5");
            var calculation = CreateCalculation();

            Assert.ThrowsAny<UserOrientedException>(
                () =>
                {
                    calculation.Add(commit);
                    calculation.GetResult();
                });
        }

        [Fact]
        public void RejectsMultipleBaseVersionsInSingleCommit()
        {
            var commit = Generate.Commit(tags: new[] { "2.3.4", "3.4.5" });
            var calculation = CreateCalculation();

            Assert.ThrowsAny<UserOrientedException>(
                () =>
                {
                    calculation.Add(commit);
                    calculation.GetResult();
                });
        }

        [Fact]
        public void IgnoresBuildMetadataInBaseVersion()
        {
            var commit = Generate.Commit(tag: "2.3.4+foo");
            var calculation = CreateCalculation();

            calculation.Add(commit);
            var result = calculation.GetResult();

            Assert.DoesNotContain("foo", result.InformationalVersion);
        }

        private static AssemblyVersionCalculation CreateCalculation()
            => new AssemblyVersionCalculation(DefaultVersioningScheme);
    }
}
