namespace Sorrend.Core.OperatingSystem;

public sealed class RelativePath : PathBase<RelativePath>
{
    public bool HasRelativeComponents
        => !IsLiteralPathRoot(Root) && Components.Contains(ParentDirectoryComponent);

    public RelativePath(string path)
        : base(path)
    {
    }

    public RelativePath(string path, bool isPosix)
        : base(path, isPosix)
    {
    }

    private RelativePath(string root, IReadOnlyList<string> components, bool isPosix)
        : base(root, components, isPosix)
    {
    }

    protected override RelativePath CopyWith(string root, IReadOnlyList<string> components)
        => new(root, components, IsPosix);
}
