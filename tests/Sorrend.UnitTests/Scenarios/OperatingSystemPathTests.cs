using Sorrend.Core.OperatingSystem;

namespace Sorrend.UnitTests.Scenarios;

public class OperatingSystemPathTests
{
    [Theory]
    [InlineData(false, @"C:\foo\bar")]
    [InlineData(false, @"\\server\share\foo\bar")]
    [InlineData(false, @"\\.\C:\foo\bar")]
    [InlineData(false, @"\\?\C:\foo\bar")]
    [InlineData(false, @"\??\C:\foo\bar")]
    [InlineData(false, @"foo\bar")]
    [InlineData(false, @"..\foo\bar")]
    [InlineData(false, @"\foo\bar")]
    [InlineData(false, "")]
    [InlineData(true, "/foo/bar")]
    [InlineData(true, "//foo/bar")]
    [InlineData(true, "foo/bar")]
    public void ToString_ReturnsPath(bool posix, string path)
    {
        var sut = new TestablePath(path, posix);
        var result = sut.ToString();
        Assert.Equal(path, result);
    }

    [Theory]
    [InlineData(false, @"foo\bar")]
    [InlineData(false, @"\foo\bar")]
    [InlineData(true, "foo/bar")]
    public void AbsolutePath_RejectsRelativePaths(bool posix, string path)
    {
        Assert.Throws<ArgumentException>(() => _ = new AbsolutePath(path, posix));
    }

    [Theory]
    [InlineData("C:/foo/bar", @"C:\foo\bar")]
    [InlineData("//server/share/foo/bar", @"\\server\share\foo\bar")]
    [InlineData("//./C:/foo/bar", @"\\.\C:\foo\bar")]
    [InlineData(@"\\?\C:/foo/bar", @"\\?\C:\foo\bar")]
    [InlineData(@"\??\C:/foo/bar", @"\??\C:\foo\bar")]
    [InlineData("/", @"\")]
    [InlineData("foo/bar", @"foo\bar")]
    [InlineData("/foo/bar", @"\foo\bar")]
    public void NormalizesDirectorySeparators_ForNonPosixPaths(string path, string expected)
    {
        var sut = new TestablePath(path, false);
        Assert.Equal(expected, sut.ToString());
    }

    [Theory]
    [InlineData(@"/foo\bar")]
    [InlineData(@"foo\bar")]
    [InlineData(@"\foo\bar")]
    public void DoesNotNormalizeDirectorySeparators_ForPosixPaths(string path)
    {
        var sut = new TestablePath(path, true);
        Assert.Equal(path, sut.ToString());
    }

    [Theory]
    [InlineData(false, @"C:\foo\bar\", @"C:\foo\bar")]
    [InlineData(false, @"\\server\share\", @"\\server\share")]
    [InlineData(false, @"\\?\C:\foo\bar\", @"\\?\C:\foo\bar")]
    [InlineData(false, @"\??\C:\foo\bar\", @"\??\C:\foo\bar")]
    [InlineData(false, @"foo\bar\", @"foo\bar")]
    [InlineData(true, "/foo/bar/", "/foo/bar")]
    public void RemovesTrailingDirectorySeparator(bool posix, string path, string expected)
    {
        var sut = new TestablePath(path, posix);
        Assert.Equal(expected, sut.ToString());
    }

    [Theory]
    [InlineData(false, @"C:\")]
    [InlineData(false, @"\\.\")]
    [InlineData(false, @"\\?\")]
    [InlineData(false, @"\??\")]
    [InlineData(false, @"\")]
    [InlineData(true, "/")]
    [InlineData(true, "//")]
    public void DoesNotRemoveRootTrailingDirectorySeparator(bool posix, string path)
    {
        var sut = new TestablePath(path, posix);
        Assert.Equal(path, sut.ToString());
    }

    [Fact]
    public void AppendsTrailingDirectorySeparator_ToDriveName()
    {
        var sut = new TestablePath("C:", false);
        Assert.Equal(@"C:\", sut.ToString());
    }

    [Theory]
    [InlineData(false, @"C:\\foo\\\bar\\", @"C:\foo\bar")]
    [InlineData(false, @"\\?\C:\foo\\bar", @"\\?\C:\foo\bar")]
    [InlineData(false, @"\??\C:\foo\\bar", @"\??\C:\foo\bar")]
    [InlineData(false, @"foo\\bar", @"foo\bar")]
    [InlineData(false, @"\foo\\bar", @"\foo\bar")]
    [InlineData(false, @"\\foo", @"\foo")]
    [InlineData(true, "///foo//bar", "/foo/bar")]
    [InlineData(true, "//foo//bar", "//foo/bar")]
    public void CollapsesDirectorySeparators(bool posix, string path, string expected)
    {
        var sut = new TestablePath(path, posix);
        Assert.Equal(expected, sut.ToString());
    }

    [Theory]
    [InlineData(false, @"C:\.\foo\.\bar\.", @"C:\foo\bar")]
    [InlineData(false, @"C:\foo\bar\..\baz\..\..\spam\ham\..", @"C:\spam")]
    [InlineData(false, @"\\server\share\.\foo\..\bar", @"\\server\share\bar")]
    [InlineData(false, @"\\.\C:\.\foo\..\bar", @"\\.\C:\bar")]
    [InlineData(true, @"/./foo\bar/../baz", "/baz")]
    public void RemovesRelativeComponents_ForAbsolutePaths(bool posix, string path, string expected)
    {
        var sut = new TestablePath(path, posix);
        Assert.Equal(expected, sut.ToString());
    }

    [Theory]
    [InlineData(false, @"C:\..\..", @"C:\")]
    [InlineData(false, @"C:\..\foo", @"C:\foo")]
    [InlineData(false, @"\\server\share\..", @"\\server\share")]
    [InlineData(false, @"\\.\..", @"\\.\")]
    [InlineData(false, @"\\.\C:\..", @"\\.\")]
    [InlineData(false, @"\\.\UNC\server\share\..", @"\\.\UNC\server\share")]
    [InlineData(false, @"\..", @"\")]
    [InlineData(true, "/..", "/")]
    [InlineData(true, "//..", "//")]
    public void DoublePeriods_DoNotRemoveRootComponents(bool posix, string path, string expected)
    {
        var sut = new TestablePath(path, posix);
        Assert.Equal(expected, sut.ToString());
    }

    [Fact]
    public void TreatsExtendedLengthPathPrefix_WithForwardSlashes_AsDevicePathPrefix()
    {
        var sut = new TestablePath("//?/foo//./bar/..", false);
        Assert.Equal(@"\\.\foo", sut.ToString());
    }

    [Theory]
    [InlineData(@"\\?\foo\.\bar\..")]
    [InlineData(@"\??\foo\.\bar\..")]
    public void PreservesRelativeComponents_ForLiteralPaths(string path)
    {
        var sut = new TestablePath(path, false);
        Assert.Equal(path, sut.ToString());
    }

    [Theory]
    [InlineData(false, @"foo\.\bar\spam\..\ham\..\..\quux", @"foo\quux")]
    [InlineData(false, @"foo\..\..\..", @"..\..")]
    [InlineData(false, @".\..\foo\bar\.", @"..\foo\bar")]
    [InlineData(true, "./foo/../bar", "bar")]
    [InlineData(true, @"\foo\bar/../baz", "baz")]
    public void RemovesRelativeComponents_ForRelativePaths(bool posix, string path, string expected)
    {
        var sut = new TestablePath(path, posix);
        Assert.Equal(expected, sut.ToString());
    }

    [Theory]
    [InlineData(false, @"C:\foo\bar", @"..\spam\ham", @"C:\foo\spam\ham")]
    [InlineData(false, @"\\server\share", @"..\spam\ham", @"\\server\share\spam\ham")]
    [InlineData(false, @"\\?\foo\.\bar\..", @"..\spam\ham", @"\\?\foo\.\bar\..\..\spam\ham")]
    [InlineData(false, @"foo\bar", @"..\spam\ham", @"foo\spam\ham")]
    [InlineData(false, "", "", "")]
    [InlineData(true, "/foo/bar", "../spam/ham", "/foo/spam/ham")]
    [InlineData(true, "foo/bar", "../spam/ham", "foo/spam/ham")]
    public void AppendsDirectoryRelativePath(bool posix, string left, string right, string expected)
    {
        var sut = new TestablePath(left, posix);
        var result = sut.Append(right);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData(@"C:\foo\bar", @"\spam\ham", @"C:\spam\ham")]
    [InlineData(@"\\server\share\foo\bar", @"\spam\ham", @"\\server\share\spam\ham")]
    [InlineData(@"\\?\foo\.\bar\..", @"\spam\ham", @"\\?\spam\ham")]
    [InlineData(@"..\foo\bar", @"\spam\ham", @"\spam\ham")]
    [InlineData(@"\foo\bar", @"\spam\ham", @"\spam\ham")]
    public void AppendingDriveRootRelativePath_OverridesOriginalComponents(string left, string right, string expected)
    {
        var sut = new TestablePath(left, false);
        var result = sut.Append(right);
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void RejectsAppendingRelativePath_WhenOperatingSystemDiffer()
    {
        var leftSut = new AbsolutePath(@"C:\foo\bar", false);
        var rightSut = new RelativePath("spam/ham", true);
        Assert.Throws<ArgumentException>(() => leftSut.Append(rightSut));
    }

    [Theory]
    [InlineData(false, @"C:\foo\bar", @"D:\spam\ham")]
    [InlineData(false, @"\\?\foo\.\bar\..", @"\\?\.\spam\..\ham")]
    [InlineData(false, @"foo\bar", @"C:\spam\ham")]
    [InlineData(true, "/foo/bar", "/spam/ham")]
    [InlineData(true, "/foo/bar", "//spam/ham")]
    [InlineData(true, "//foo/bar", "/spam/ham")]
    [InlineData(true, "foo/bar", "/spam/ham")]
    public void AppendingAbsolutePath_OverridesOriginalPath(bool posix, string left, string right)
    {
        var sut = new TestablePath(left, posix);
        var result = sut.Append(right);
        Assert.Equal(right, result.ToString());
    }

    [Theory]
    [InlineData(false, @"C:\")]
    [InlineData(false, @"C:\foo\bar")]
    [InlineData(false, @"foo\bar")]
    [InlineData(false, "")]
    [InlineData(true, "/foo/bar")]
    public void ConsidersSamePathsEqual(bool posix, string path)
    {
        var leftSut = new TestablePath(path, posix);
        var rightSut = new TestablePath(path, posix);
        var equality = leftSut.Equals(rightSut);
        Assert.True(equality);
    }

    [Theory]
    [InlineData(false, @"C:\foo\bar", @"C:\spam\ham")]
    [InlineData(false, @"C:\foo\bar", @"D:\foo\bar")]
    [InlineData(false, @"\\?\foo\..\spam\ham", @"\\?\spam\ham")]
    [InlineData(false, @"foo\bar", @"\foo\bar")]
    [InlineData(false, @"\foo\bar", @"C:\foo\bar")]
    [InlineData(true, "/foo/bar", "/spam/ham")]
    [InlineData(true, "/foo/bar", "//foo/bar")]
    [InlineData(true, "foo/bar", "/foo/bar")]
    public void ConsidersDifferentPathsUnequal(bool posix, string left, string right)
    {
        var leftSut = new TestablePath(left, posix);
        var rightSut = new TestablePath(right, posix);
        var equality = leftSut.Equals(rightSut);
        Assert.False(equality);
    }

    [Theory]
    [InlineData(@"C:\foo\bar", @"C:\FOO\BAR")]
    [InlineData(@"\\?\foo\bar", @"\\?\FOO\BAR")]
    [InlineData(@"\??\foo\bar", @"\??\FOO\BAR")]
    [InlineData(@"C:\bɐr", @"C:\BⱯR")]
    public void Equality_IsCaseInsensitive_ForNonPosixPaths(string left, string right)
    {
        var leftSut = new TestablePath(left, false);
        var rightSut = new TestablePath(right, false);

        var equality = leftSut.Equals(rightSut);
        var leftHash = leftSut.GetHashCode();
        var rightHash = rightSut.GetHashCode();

        Assert.True(equality);
        Assert.Equal(rightHash, leftHash);
    }

    [Fact]
    public void Equality_IsCaseSensitive_ForPosixPaths()
    {
        var leftSut = new TestablePath("/foo/bar", true);
        var rightSut = new TestablePath("/FOO/BAR", true);
        var equality = leftSut.Equals(rightSut);
        Assert.False(equality);
    }

    [Fact]
    public void ConsidersEquivalentPathsUnequal_WhenOperatingSystemsDiffer()
    {
        var leftSut = new TestablePath(@"foo\bar", false);
        var rightSut = new TestablePath("foo/bar", true);
        var equality = leftSut.Equals(rightSut);
        Assert.False(equality);
    }

    [Theory]
    [InlineData(false, @"C:\foo\bar", @"C:\foo")]
    [InlineData(false, @"\\?\foo\..", @"\\?\foo")]
    [InlineData(false, @"foo\bar", "foo")]
    [InlineData(false, "", "..")]
    [InlineData(false, "..", @"..\..")]
    [InlineData(true, "/foo/bar", "/foo")]
    public void ParentDirectory_ReturnsPathToPreviousComponent(bool posix, string path, string expected)
    {
        var sut = new TestablePath(path, posix);
        var result = sut.ParentDirectory;
        Assert.Equal(expected, result.ToString());
        Assert.Equal(posix, result.IsPosix);
    }

    [Theory]
    [InlineData(false, @"C:\")]
    [InlineData(false, @"\\server\share")]
    [InlineData(false, @"\")]
    [InlineData(true, "/")]
    [InlineData(true, "//")]
    public void ParentDirectory_ReturnsSamePath_ForRootPaths(bool posix, string path)
    {
        var sut = new TestablePath(path, posix);
        var result = sut.ParentDirectory;
        Assert.Same(sut, result);
    }

    [Theory]
    [InlineData(@"C:\foo\bar", "bar")]
    [InlineData(@"\\?\foo\bar\..", "..")]
    [InlineData("foo", "foo")]
    [InlineData(@"..\foo", "foo")]
    public void BaseName_ReturnsLastComponent(string path, string expected)
    {
        var sut = new TestablePath(path, false);
        var result = sut.BaseName;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"C:\")]
    [InlineData(@"\\server\share")]
    [InlineData(@"\\.\UNC\server\share")]
    [InlineData("")]
    [InlineData(@"..\..")]
    public void BaseName_Throws_WhenMissingOrUnknown(string path)
    {
        var sut = new TestablePath(path, false);
        Assert.Throws<InvalidOperationException>(() => sut.BaseName);
    }

    [Theory]
    [InlineData(@"C:\foo\bar.baz", "bar")]
    [InlineData(@"C:\foo\bar.spam.ham", "bar")]
    [InlineData(@"C:\foo\bar", "bar")]
    public void BaseNameWithoutExtensions_RemovesAllExtensions(string path, string expected)
    {
        var sut = new TestablePath(path, false);
        var result = sut.BaseNameWithoutExtensions;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"C:\foo\.bar", ".bar")]
    [InlineData(@"C:\foo\.bar.baz", ".bar")]
    [InlineData(@"\\?\foo\.", ".")]
    [InlineData(@"\\?\foo\..", "..")]
    [InlineData(@"\\?\foo\..bar", "..bar")]
    [InlineData(@"\\?\foo\...bar.baz", "...bar")]
    public void BaseNameWithoutExtensions_IgnoresLeadingPeriods(string path, string expected)
    {
        var sut = new TestablePath(path, false);
        var result = sut.BaseNameWithoutExtensions;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BaseNameWithoutExtensions_Throws_WhenBaseNameIsMissingOrUnknown()
    {
        var sut = new TestablePath(@"C:\", false);
        Assert.Throws<InvalidOperationException>(() => sut.BaseNameWithoutExtensions);
    }

    [Theory]
    [InlineData(@"C:\foo\bar.baz", "baz", true)]
    [InlineData(@"C:\foo\bar.spam.ham", "ham", true)]
    [InlineData(@"C:\foo\bar.spam.ham", "spam.ham", true)]
    [InlineData(@"\\?\foo\...bar.baz", "baz", true)]
    [InlineData(@"C:\foo\bar.baz", "quux", false)]
    [InlineData(@"C:\foo\bar.baz", "az", false)]
    [InlineData(@"C:\foo\bar.baz", "bar.baz", false)]
    [InlineData(@"C:\foo\bar.spam.ham", "spam", false)]
    [InlineData(@"\\?\foo\...bar.baz", "bar.baz", false)]
    [InlineData(@"\\?\foo\bar.baz.", "baz", false)]
    [InlineData(@"C:\foo\bar", "bar", false)]
    [InlineData(@"C:\foo\.bar", "bar", false)]
    [InlineData(@"C:\", "foo", false)]
    [InlineData(@"..\..", "foo", false)]
    [InlineData("", "foo", false)]
    public void EndsWithExtension_ComparesTrailingExtensions(string path, string extension, bool expected)
    {
        var sut = new TestablePath(path, false);
        var result = sut.EndsWithExtension(extension);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EndsWithExtension_IgnoresArgumentLeadingPeriod()
    {
        var sut = new TestablePath(@"C:\foo\bar.baz", false);
        var result = sut.EndsWithExtension(".baz");
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("baz.")]
    [InlineData("..baz")]
    public void EndsWithExtension_Throws_WhenArgumentIsMalformed(string extension)
    {
        var sut = new TestablePath(@"C:\foo\bar.baz", false);
        Assert.Throws<ArgumentException>(() => sut.EndsWithExtension(extension));
    }

    [Theory]
    [InlineData(false, @"C:\foo\bar.bɐz", "BⱯZ")]
    [InlineData(true, "/foo/bar.bɐz", "BⱯZ")]
    public void EndsWithExtension_IgnoresCase(bool posix, string path, string extension)
    {
        var sut = new TestablePath(path, posix);
        var result = sut.EndsWithExtension(extension);
        Assert.True(result);
    }

    [Theory]
    [InlineData(@"foo\bar", false)]
    [InlineData("", false)]
    [InlineData(@"..\..", true)]
    public void HasRelativeComponents_ReturnsTrue_WhenPathHasDoublePeriods(string path, bool expected)
    {
        var sut = new RelativePath(path, false);
        var result = sut.HasRelativeComponents;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HasRelativeComponents_ReturnsFalse_ForLiteralPaths()
    {
        var sut = new RelativePath(@"\\?\..", false);
        var result = sut.HasRelativeComponents;
        Assert.False(result);
    }

    private class TestablePath : PathBase<TestablePath>
    {
        public TestablePath(string path, bool isPosix)
            : base(path, isPosix)
        {
        }

        private TestablePath(string root, IReadOnlyList<string> components, bool isPosix)
            : base(root, components, isPosix)
        {
        }

        protected override TestablePath CopyWith(string root, IReadOnlyList<string> components)
            => new(root, components, IsPosix);
    }
}
