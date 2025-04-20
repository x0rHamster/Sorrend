using Sorrend.Core.Versions;
using Xunit;

namespace Sorrend.UnitTests.Scenarios
{
    public class SemanticVersionIncrementTests
    {
        [Fact]
        public void ReturnsSameReleaseVersionIdentifiers_WithoutIncrements()
        {
            var currentVersion = SemanticVersion.Parse("2.3.4");
            var versionIncrement = new SemanticVersionIncrement();

            Assert.Equal(2, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(3, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(4, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Null(versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, null, "dev"));
            Assert.Null(versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Fact]
        public void ReturnsSamePreReleaseVersionIdentifiers_WithoutIncrements()
        {
            var currentVersion = SemanticVersion.Parse("2.3.4-foo.5");
            var versionIncrement = new SemanticVersionIncrement();

            Assert.Equal(2, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(3, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(4, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Equal("foo", versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, "foo", "dev"));
            Assert.Equal(5, versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, 5));
        }

        [Fact]
        public void ReturnsDefaultCounterPreReleaseIdentifier_WithoutIncrements()
        {
            var currentVersion = SemanticVersion.Parse("2.3.4-foo");
            var versionIncrement = new SemanticVersionIncrement();

            Assert.Equal(1, versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Minor, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Patch, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Minor, 2, 4, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Patch, 2, 4, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Minor, 2, 4, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Patch, 2, 3, 5)]
        public void PreReleases_BasedOnRelease_UseLargestNormalVersion(
            SemanticVersioningReleaseType firstPreReleaseType,
            SemanticVersioningReleaseType secondPreReleaseType,
            int expectedMajorVersion,
            int expectedMinorVersion,
            int expectedPatchVersion)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddPreRelease(secondPreReleaseType);
            versionIncrement.AddPreRelease(firstPreReleaseType);

            Assert.Equal(expectedMajorVersion, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(expectedMinorVersion, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(expectedPatchVersion, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Equal("dev", versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, null, "dev"));
            Assert.Equal(2, versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major)]
        [InlineData(SemanticVersioningReleaseType.Minor)]
        [InlineData(SemanticVersioningReleaseType.Patch)]
        public void PreReleases_BasedOnPreRelease_LeaveSameNormalVersion(
            SemanticVersioningReleaseType secondPreReleaseType)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4-foo.5");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddPreRelease(secondPreReleaseType);
            versionIncrement.AddPreRelease(SemanticVersioningReleaseType.Minor);

            Assert.Equal(2, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(3, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(4, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Equal("foo", versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, "foo", "dev"));
            Assert.Equal(7, versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, 5));
        }

        [Fact]
        public void IncrementsDefaultCounterPreReleaseIdentifier()
        {
            var currentVersion = SemanticVersion.Parse("2.3.4-foo");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddPreRelease(SemanticVersioningReleaseType.Patch);

            Assert.Equal(2, versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Major, 4, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Minor, 3, 1, 0)]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Patch, 3, 0, 1)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Minor, 2, 5, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Patch, 2, 4, 1)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Minor, 2, 4, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Patch, 2, 3, 6)]
        public void Releases_BasedOnRelease_IncrementNormalVersion(
            SemanticVersioningReleaseType firstReleaseType,
            SemanticVersioningReleaseType secondReleaseType,
            int expectedMajorVersion,
            int expectedMinorVersion,
            int expectedPatchVersion)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddRelease(secondReleaseType);
            versionIncrement.AddRelease(firstReleaseType);

            Assert.Equal(expectedMajorVersion, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(expectedMinorVersion, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(expectedPatchVersion, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Null(versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, null, "dev"));
            Assert.Null(versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, 2, 5, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, 2, 4, 1)]
        public void Releases_BasedOnPreRelease_IncrementNormalVersion(
            SemanticVersioningReleaseType secondReleaseType,
            int expectedMajorVersion,
            int expectedMinorVersion,
            int expectedPatchVersion)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4-foo.5");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddRelease(secondReleaseType);
            versionIncrement.AddRelease(SemanticVersioningReleaseType.Minor);

            Assert.Equal(expectedMajorVersion, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(expectedMinorVersion, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(expectedPatchVersion, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Null(versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, "foo", "dev"));
            Assert.Null(versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, 5));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Minor, 2, 4, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Minor, 2, 4, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Patch, 2, 3, 5)]
        public void ReleasesNotGreaterPreReleaseBasedOnRelease(
            SemanticVersioningReleaseType preReleaseType,
            SemanticVersioningReleaseType releaseType,
            int expectedMajorVersion,
            int expectedMinorVersion,
            int expectedPatchVersion)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddRelease(releaseType);
            versionIncrement.AddPreRelease(preReleaseType);

            Assert.Equal(expectedMajorVersion, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(expectedMinorVersion, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(expectedPatchVersion, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Null(versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, null, "dev"));
            Assert.Null(versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Minor, 3, 1, 0)]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Patch, 3, 0, 1)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Patch, 2, 4, 1)]
        public void Release_TreatsGreaterPreRelease_BasedOnRelease_AsRelease(
            SemanticVersioningReleaseType preReleaseType,
            SemanticVersioningReleaseType releaseType,
            int expectedMajorVersion,
            int expectedMinorVersion,
            int expectedPatchVersion)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddRelease(releaseType);
            versionIncrement.AddPreRelease(preReleaseType);

            Assert.Equal(expectedMajorVersion, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(expectedMinorVersion, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(expectedPatchVersion, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Null(versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, null, "dev"));
            Assert.Null(versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major, SemanticVersioningReleaseType.Major, 4, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, SemanticVersioningReleaseType.Minor, 2, 5, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, SemanticVersioningReleaseType.Patch, 2, 4, 0)]
        public void PreReleases_MadeAfterRelease_BasedOnRelease_AreBasedOnReleaseMade(
            SemanticVersioningReleaseType releaseType,
            SemanticVersioningReleaseType secondPreReleaseType,
            int expectedMajorVersion,
            int expectedMinorVersion,
            int expectedPatchVersion)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddPreRelease(secondPreReleaseType);
            versionIncrement.AddPreRelease(SemanticVersioningReleaseType.Minor);
            versionIncrement.AddRelease(releaseType);

            Assert.Equal(expectedMajorVersion, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(expectedMinorVersion, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(expectedPatchVersion, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Equal("dev", versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, null, "dev"));
            Assert.Equal(2, versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, null));
        }

        [Theory]
        [InlineData(SemanticVersioningReleaseType.Major, 3, 0, 0)]
        [InlineData(SemanticVersioningReleaseType.Minor, 2, 5, 0)]
        [InlineData(SemanticVersioningReleaseType.Patch, 2, 5, 0)]
        public void PreRelease_MadeAfterRelease_BasedOnPreRelease_IsBasedOnReleaseMade(
            SemanticVersioningReleaseType secondPreReleaseType,
            int expectedMajorVersion,
            int expectedMinorVersion,
            int expectedPatchVersion)
        {
            var currentVersion = SemanticVersion.Parse("2.3.4-foo.5");
            var versionIncrement = new SemanticVersionIncrement();

            versionIncrement.AddPreRelease(secondPreReleaseType);
            versionIncrement.AddPreRelease(SemanticVersioningReleaseType.Minor);
            versionIncrement.AddRelease(SemanticVersioningReleaseType.Minor);

            Assert.Equal(expectedMajorVersion, versionIncrement.GetMajorVersion(currentVersion));
            Assert.Equal(expectedMinorVersion, versionIncrement.GetMinorVersion(currentVersion));
            Assert.Equal(expectedPatchVersion, versionIncrement.GetPatchVersion(currentVersion));
            Assert.Equal("foo", versionIncrement.GetPrefixPreReleaseIdentifier(currentVersion, "foo", "dev"));
            Assert.Equal(2, versionIncrement.GetCounterPreReleaseIdentifier(currentVersion, 5));
        }
    }
}
