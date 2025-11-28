using System.IO;

namespace Sorrend.Core.OperatingSystem;

public partial class PathBase<TPath>
{
    private const char AlternativeWindowsDirectorySeparator = '/';

    private static bool OperatingSystemIsPosixCompatible
        => Path.DirectorySeparatorChar == PosixDirectorySeparator;

    protected PathBase(string path)
        : this(path, OperatingSystemIsPosixCompatible)
    {
    }

    protected PathBase(string path, bool isPosix)
    {
        var (root, components) = SplitPath(path, isPosix);
        components = RemoveRelativeComponents(components, root);

        Root = root;
        Components = components;
        IsPosix = isPosix;
    }

    [SuppressMessage(
        "Usage",
        "CA2225:Operator overloads have named alternates",
        Justification = "The alternative method is named Append() instead of Divide()")]
    public static TPath operator /(PathBase<TPath> left, string right)
        => left.Append(right);

    public TPath Append(string path)
    {
        var (otherRoot, otherComponents) = SplitPath(path, IsPosix);
        return Append(otherRoot, otherComponents);
    }

    protected TPath Append(string otherRoot, IReadOnlyList<string> otherComponents)
    {
        var root = JoinRoots(Root, otherRoot);

        if (otherRoot != string.Empty)
        {
            return CopyWith(root, otherComponents);
        }

        var components = PathBase.JoinComponents(Components, otherComponents);
        components = RemoveRelativeComponents(components, root);

        return CopyWith(root, components);
    }

    private static (string Root, IReadOnlyList<string> Components) SplitPath(string path, bool isPosix)
    {
        var root = GetPathRoot(path, isPosix);
        var components = GetPathComponents(path, root.Length, isPosix);
        return (root, components);
    }

    private static string GetPathRoot(string path, bool isPosix)
    {
        if (isPosix)
        {
            return GetPosixPathRoot(path);
        }

        if (TryGetDriveRoot(path, out var driveRoot))
        {
            return driveRoot;
        }

        if (path.Length == 0 || !IsWindowsDirectorySeparator(path[0]))
        {
            return string.Empty;
        }

        if (TryGetDevicePathRoot(path, out var devicePathRoot))
        {
            return TryGetUncDevicePathRoot(path, devicePathRoot, out var uncDevicePathRoot)
                ? uncDevicePathRoot
                : devicePathRoot;
        }

        if (path.StartsWith(NtObjectPathRoot, StringComparison.Ordinal))
        {
            return NtObjectPathRoot;
        }

        return TryGetUncPathRoot(path, out var uncPathRoot)
            ? uncPathRoot
            : DriveRootRelativePathRoot;
    }

    private static string GetPosixPathRoot(string path)
    {
        if (
            path is [PosixDirectorySeparator, PosixDirectorySeparator, ..]
            and not [_, _, PosixDirectorySeparator, ..])
        {
            return ImplementationSpecificPosixPathRoot;
        }

        return path is [PosixDirectorySeparator, ..]
            ? PosixPathRoot
            : string.Empty;
    }

    private static bool TryGetDriveRoot(string path, out string result)
    {
        const int driveRootLength = 3;

        if (path is not [>= 'A' and <= 'Z' or >= 'a' and <= 'z', ':', ..])
        {
            result = string.Empty;
            return false;
        }

        if (
            path.Length >= driveRootLength
            && !IsWindowsDirectorySeparator(path[driveRootLength - 1]))
        {
            result = string.Empty;
            return false;
        }

        result = char.ToUpperInvariant(path[0]) + ":" + WindowsDirectorySeparator;
        return true;
    }

    private static bool TryGetDevicePathRoot(string path, out string result)
    {
        if (path.Length < DevicePathRoot.Length)
        {
            result = string.Empty;
            return false;
        }

        var hasDevicePathRoot = StartsWith(
            path,
            IsWindowsDirectorySeparator,
            IsWindowsDirectorySeparator,
            x => x is '.' or '?',
            IsWindowsDirectorySeparator);

        if (hasDevicePathRoot)
        {
            result = path.StartsWith(ExtendedLengthPathRoot, StringComparison.Ordinal)
                ? ExtendedLengthPathRoot
                : DevicePathRoot;

            return true;
        }

        result = string.Empty;
        return false;
    }

    private static bool TryGetUncDevicePathRoot(string path, string devicePathRoot, out string result)
    {
        const int uncDevicePathRootLength = 8;

        if (
            path is [_, _, _, _, 'U', 'N', 'C', _, ..]
            && IsWindowsDirectorySeparator(path[uncDevicePathRootLength - 1])
            && TryGetUncPathVolume(path, uncDevicePathRootLength, out result))
        {
            result = devicePathRoot + "UNC" + WindowsDirectorySeparator + result;
            return true;
        }

        result = string.Empty;
        return false;
    }

    private static bool TryGetUncPathRoot(string path, out string result)
    {
        if (
            path.Length > UncPathRootPrefix.Length
            && IsWindowsDirectorySeparator(path[0])
            && IsWindowsDirectorySeparator(path[1])
            && TryGetUncPathVolume(path, UncPathRootPrefix.Length, out result))
        {
            result = UncPathRootPrefix + result;
            return true;
        }

        result = string.Empty;
        return false;
    }

    private static bool TryGetUncPathVolume(string path, int startIndex, out string result)
    {
        var (serverStart, serverEnd) = FindComponentRange(path, startIndex, IsWindowsDirectorySeparator);
        var (shareStart, shareEnd) = FindComponentRange(path, serverEnd, IsWindowsDirectorySeparator);

        var hasShare = shareStart < shareEnd;
        if (!hasShare)
        {
            result = string.Empty;
            return false;
        }

        var serverName = path[serverStart..serverEnd];
        var shareName = path[shareStart..shareEnd];

        if (IsRelativeComponent(serverName) || IsRelativeComponent(shareName))
        {
            result = string.Empty;
            return false;
        }

        result = serverName + WindowsDirectorySeparator + shareName;
        return true;
    }

    private static IReadOnlyList<string> GetPathComponents(string path, int rootLength, bool isPosix)
    {
        if (path.Length <= rootLength)
        {
            return [];
        }

        Func<char, bool> isDirectorySeparator = isPosix
            ? IsPosixDirectorySeparator
            : IsWindowsDirectorySeparator;

        var result = new List<string>();

        var componentEnd = rootLength;

        do
        {
            (var componentStart, componentEnd) = FindComponentRange(path, componentEnd, isDirectorySeparator);

            var hasComponent = componentStart < componentEnd;
            if (hasComponent)
            {
                result.Add(path[componentStart..componentEnd]);
            }
        }
        while (componentEnd < path.Length);

        return result;
    }

    private static string JoinRoots(string leftRoot, string rightRoot)
    {
        if (rightRoot == string.Empty)
        {
            return leftRoot;
        }

        if (leftRoot == string.Empty)
        {
            return rightRoot;
        }

        return rightRoot == DriveRootRelativePathRoot
            ? leftRoot
            : rightRoot;
    }

    private static IReadOnlyList<string> RemoveRelativeComponents(IReadOnlyList<string> components, string root)
    {
        if (IsLiteralPathRoot(root))
        {
            return components;
        }

        var result = new PathBase.LazyComponents(components);

        var parentDirectoryCount = 0;

        for (var i = components.Count - 1; i >= 0; i--)
        {
            var component = components[i];

            if (component == CurrentDirectoryComponent)
            {
                result.AsMutable().RemoveAt(i);
            }
            else if (component == ParentDirectoryComponent)
            {
                parentDirectoryCount++;
            }
            else if (parentDirectoryCount > 0)
            {
                result.AsMutable().RemoveRange(i, count: 2);
                parentDirectoryCount--;
            }
        }

        if (root != string.Empty)
        {
            result.AsMutable().RemoveRange(0, parentDirectoryCount);
        }

        return result.AsReadOnly();
    }

    private static (int Start, int End) FindComponentRange(
        string path,
        int startIndex,
        Func<char, bool> isDirectorySeparator)
    {
        int start;
        for (start = startIndex; start < path.Length; start++)
        {
            if (!isDirectorySeparator(path[start]))
            {
                break;
            }
        }

        int end;
        for (end = start; end < path.Length; end++)
        {
            if (isDirectorySeparator(path[end]))
            {
                break;
            }
        }

        return (start, end);
    }

    private static bool IsWindowsDirectorySeparator(char value)
        => value is WindowsDirectorySeparator or AlternativeWindowsDirectorySeparator;

    private static bool IsPosixDirectorySeparator(char value)
        => value == PosixDirectorySeparator;

    private static bool StartsWith(string value, params Func<char, bool>[] predicates)
    {
        if (value.Length < predicates.Length)
        {
            return false;
        }

        for (var i = 0; i < predicates.Length; i++)
        {
            if (!predicates[i](value[i]))
            {
                return false;
            }
        }

        return true;
    }
}
