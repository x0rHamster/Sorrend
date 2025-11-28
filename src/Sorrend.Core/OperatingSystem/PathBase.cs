namespace Sorrend.Core.OperatingSystem;

internal static class PathBase
{
    public static IReadOnlyList<string> PrependComponent(IReadOnlyList<string> components, string component)
        => [component, ..components];

    public static IReadOnlyList<string> RemoveLastComponent(IReadOnlyList<string> components)
    {
        var result = new string[components.Count - 1];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = components[i];
        }

        return result;
    }

    public static IReadOnlyList<string> JoinComponents(
        IReadOnlyList<string> leftComponents,
        IReadOnlyList<string> rightComponents)
    {
        return leftComponents.Count == 0
            ? rightComponents
            : rightComponents.Count == 0
                ? leftComponents
                : [..leftComponents, ..rightComponents];
    }

    public struct LazyComponents(IReadOnlyList<string> collection)
    {
        private List<string>? _value;

        public List<string> AsMutable()
            => _value ??= [..collection];

        public IReadOnlyList<string> AsReadOnly()
            => _value ?? collection;
    }
}
