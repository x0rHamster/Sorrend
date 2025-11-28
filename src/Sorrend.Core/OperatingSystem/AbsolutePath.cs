using System.IO;

namespace Sorrend.Core.OperatingSystem;

[SuppressMessage(
    "Major Code Smell",
    "S4050:Operators should be overloaded consistently",
    Justification = "The base class provides the necessary operators")]
public sealed class AbsolutePath : PathBase<AbsolutePath>
{
    public static AbsolutePath LocalAppDataDirectory
        => new(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    public static AbsolutePath TempDirectory
        => new(Path.GetTempPath());

    public static AbsolutePath WorkingDirectory
        => new(Directory.GetCurrentDirectory());

    public static AbsolutePath GetAssemblyDirectory<TAssemblyType>()
        => new AbsolutePath(typeof(TAssemblyType).Assembly.Location).ParentDirectory;

    public AbsolutePath(string path)
        : base(path)
    {
        ValidateCreated();
    }

    public AbsolutePath(string path, bool isPosix)
        : base(path, isPosix)
    {
        ValidateCreated();
    }

    private AbsolutePath(string root, IReadOnlyList<string> components, bool isPosix)
        : base(root, components, isPosix)
    {
        ValidateCreated();
    }

    private void ValidateCreated()
    {
        if (IsRelativePathRoot(Root))
        {
            throw new ArgumentException($"\"{this}\" is not an absolute path.");
        }
    }

    protected override AbsolutePath CopyWith(string root, IReadOnlyList<string> components)
        => new(root, components, IsPosix);

    [SuppressMessage(
        "Usage",
        "CA2225:Operator overloads have named alternates",
        Justification = "The alternative method is named Append() instead of Divide()")]
    public static AbsolutePath operator /(AbsolutePath left, RelativePath right)
        => left.Append(right);

    public AbsolutePath Append(RelativePath path)
    {
        if (IsPosix != path.IsPosix)
        {
            throw new ArgumentException("POSIX paths cannot be joined with non-POSIX ones.", nameof(path));
        }

        return Append(path.Root, path.Components);
    }
}
