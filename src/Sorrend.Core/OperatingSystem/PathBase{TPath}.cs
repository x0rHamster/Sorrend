using System.Text;
using Sorrend.Core.Utilities;

namespace Sorrend.Core.OperatingSystem;

[SuppressMessage(
    "Major Code Smell",
    "S4035:Classes implementing \"IEquatable<T>\" should be sealed",
    Justification = "This implementation is designed specifically for derived classes")]
public abstract partial class PathBase<TPath> : IEquatable<TPath>
    where TPath : PathBase<TPath>
{
    private const string DriveRootRelativePathRoot = @"\";
    private const string UncPathRootPrefix = @"\\";
    private const string DevicePathRoot = @"\\.\";
    private const string ExtendedLengthPathRoot = @"\\?\";
    private const string NtObjectPathRoot = @"\??\";
    private const string PosixPathRoot = "/";
    private const string ImplementationSpecificPosixPathRoot = "//";

    private const string CurrentDirectoryComponent = ".";
    protected const string ParentDirectoryComponent = "..";

    private const char WindowsDirectorySeparator = '\\';
    private const char PosixDirectorySeparator = '/';

    public string Root { get; }

    public IReadOnlyList<string> Components { get; }

    public bool IsPosix { get; }

    public TPath ParentDirectory
    {
        get
        {
            if (Root == string.Empty)
            {
                var components = Components is [not ParentDirectoryComponent, ..]
                    ? PathBase.RemoveLastComponent(Components)
                    : PathBase.PrependComponent(Components, "..");

                return CopyWith(Root, components);
            }

            if (Components.Count > 0)
            {
                var components = PathBase.RemoveLastComponent(Components);
                return CopyWith(Root, components);
            }

            return (TPath)this;
        }
    }

    public string BaseName
    {
        get
        {
            if (Components.Count == 0)
            {
                throw new InvalidOperationException(
                    Root == string.Empty
                        ? "The path is empty."
                        : $"The root directory \"{this}\" does not have a name.");
            }

            var baseName = Components[^1];

            if (baseName == ParentDirectoryComponent && !IsLiteralPathRoot(Root))
            {
                throw new InvalidOperationException($"\"{this}\" refers to the parent directory with an unknown name.");
            }

            return baseName;
        }
    }

    public string BaseNameWithoutExtensions
    {
        get
        {
            var baseName = BaseName;

            for (var i = 0; i < baseName.Length; i++)
            {
                if (baseName[i] == '.')
                {
                    continue;
                }

                var extensionIndex = baseName.IndexOf('.', i);
                return extensionIndex != -1
                    ? baseName[..extensionIndex]
                    : baseName;
            }

            return baseName;
        }
    }

    protected PathBase(string root, IReadOnlyList<string> components, bool isPosix)
    {
        Root = root;
        Components = components;
        IsPosix = isPosix;
    }

    protected abstract TPath CopyWith(string root, IReadOnlyList<string> components);

    public bool EndsWithExtension(string extension)
    {
        if (extension is "" or ".")
        {
            throw new ArgumentException("The extension cannot be empty.", nameof(extension));
        }

        if (extension.Contains("..") || extension[^1] == '.')
        {
            throw new ArgumentException($"The extension \"{extension}\" is malformed.", nameof(extension));
        }

        if (Components.Count == 0)
        {
            return false;
        }

        var baseName = Components[^1];

        var baseNameExtensionIndex = baseName.Length
            - extension.Length
            - (extension[0] == '.' ? 0 : 1);

        return baseNameExtensionIndex >= 1
            && baseName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)
            && baseName.AsSpan(baseNameExtensionIndex - 1) is [not '.', '.', ..];
    }

    public static bool operator ==(PathBase<TPath>? left, PathBase<TPath>? right)
        => left?.Equals(right as TPath) ?? right is null;

    public static bool operator !=(PathBase<TPath>? left, PathBase<TPath>? right)
        => !(left?.Equals(right as TPath) ?? right is null);

    public bool Equals(TPath? other)
    {
        var comparisonType = GetComparisonType(IsPosix);
        var comparer = comparisonType.ToComparer();

        return IsPosix == other?.IsPosix
            && string.Equals(Root, other.Root, comparisonType)
            && Components.SequenceEqual(other.Components, comparer);
    }

    public override bool Equals(object? obj)
        => Equals(obj as TPath);

    public override int GetHashCode()
    {
        unchecked
        {
            var comparer = GetComparisonType(IsPosix).ToComparer();

            var hashCode = comparer.GetHashCode(Root);

            foreach (var component in Components)
            {
                hashCode = (hashCode * 397) ^ comparer.GetHashCode(component);
            }

            hashCode = (hashCode * 397) ^ IsPosix.GetHashCode();

            return hashCode;
        }
    }

    public override string ToString()
    {
        var directorySeparator = IsPosix
            ? PosixDirectorySeparator
            : WindowsDirectorySeparator;

        var rootWithoutDirectorySeparator = Root.Length > 0 && Root[^1] != directorySeparator;

        var builder = new StringBuilder(Root);

        for (var i = 0; i < Components.Count; i++)
        {
            if (i > 0 || rootWithoutDirectorySeparator)
            {
                builder.Append(directorySeparator);
            }

            builder.Append(Components[i]);
        }

        return builder.ToString();
    }

    protected static bool IsRelativePathRoot(string root)
        => root is "" or DriveRootRelativePathRoot;

    protected static bool IsLiteralPathRoot(string root)
        => root is ExtendedLengthPathRoot or NtObjectPathRoot;

    private static bool IsRelativeComponent(string component)
        => component is CurrentDirectoryComponent or ParentDirectoryComponent;

    private static StringComparison GetComparisonType(bool isPosix)
        => isPosix
            ? StringComparison.Ordinal
            : StringComparison.InvariantCultureIgnoreCase;
}
