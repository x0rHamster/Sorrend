using System.Globalization;
using System.Text.RegularExpressions;

namespace Sorrend.Core.Utilities;

public static class StringExtensions
{
    private static readonly Regex HexNumberRegex = new("^[0-9a-fA-F]+$");

    public static bool IsInteger(this string value)
        => value.TryParseInteger(out _);

    public static int ParseInteger(this string value)
        => value.TryParseInteger(out var result)
            ? result
            : throw new FormatException($"\"{value}\" cannot be parsed as an integer.");

    public static bool TryParseCanonicalInteger(this string value, out int result)
        => value.TryParseInteger(out result)
            && result.ToString(CultureInfo.InvariantCulture) == value;

    public static bool TryParseInteger(this string value, out int result)
    {
        try
        {
            return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result);
        }
        catch (OverflowException)
        {
            result = default;
            return false;
        }
    }

    public static bool IsHexNumber(this string value)
        => HexNumberRegex.IsMatch(value);

    public static string TrimPrefix(
        this string value,
        string prefix,
        StringComparison comparisonType)
    {
        return value.StartsWith(prefix, comparisonType)
            ? value[prefix.Length..]
            : value;
    }

    [StringFormatMethod("patternFormat")]
    public static Regex FormatRegex(
        this string patternFormat,
        [RegexPattern] params object[] args)
    {
        return new Regex(
            string.Format(
                CultureInfo.InvariantCulture,
                patternFormat,
                args));
    }

    public static StringComparer ToComparer(this StringComparison comparisonType)
        => comparisonType switch
        {
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,

            _ => throw new ArgumentOutOfRangeException(
                nameof(comparisonType),
                comparisonType,
                null),
        };
}
