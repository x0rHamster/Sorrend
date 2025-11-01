namespace Sorrend.Core.Utilities;

public static class ListExtensions
{
    public static bool TryGetValue<T>(this IReadOnlyList<T> source, int index, out T value)
    {
        if (index < source.Count)
        {
            value = source[index];
            return true;
        }

        value = default!;
        return false;
    }

    public static T? GetValueOrDefault<T>(this IReadOnlyList<T> source, int index)
    {
        return index < source.Count
            ? source[index]
            : default;
    }
}
