using System.Collections;
using System.Text;

namespace Sorrend.Core.Utilities.MessageTemplates;

internal class PlaceholderToken : IToken
{
    private readonly string _identifier;

    private readonly string _name;
    private readonly int _alignment;
    private readonly string? _format;

    private int _index;

    public PlaceholderToken(string identifier)
    {
        _identifier = identifier;

        _name = identifier;

        var formatIndex = _name.IndexOf(':');
        if (formatIndex != -1)
        {
            _format = _name[(formatIndex + 1)..];
            _name = _name[..formatIndex];
        }

        var alignmentIndex = _name.IndexOf(',');
        if (alignmentIndex != -1)
        {
            _alignment = _name[(alignmentIndex + 1)..].ParseInteger();
            _name = _name[..alignmentIndex];
        }

        if (!_name.TryParseInteger(out _index))
        {
            _index = -1;
        }
    }

    public static void RecalculateIndexes(IEnumerable<IToken> tokens)
    {
        var placeholders = tokens.OfType<PlaceholderToken>().ToArray();

        var hasNamedPlaceholders = Array.Exists(placeholders, x => x._index < 0);
        if (!hasNamedPlaceholders)
        {
            return;
        }

        var indexes = new Dictionary<string, int>();
        foreach (var placeholder in placeholders)
        {
            var placeholderWasRepeated = indexes.TryGetValue(
                placeholder._name,
                out placeholder._index);

            if (placeholderWasRepeated)
            {
                continue;
            }

            placeholder._index = indexes.Count;
            indexes[placeholder._name] = placeholder._index;
        }
    }

    public void WriteTo(
        StringBuilder builder,
        object?[] arguments,
        IFormatProvider formatProvider)
    {
        if (_index >= arguments.Length)
        {
            builder.Append('{');
            builder.Append(_identifier);
            builder.Append('}');
            return;
        }

        var formatted = FormatValue(_format, arguments[_index], formatProvider);
        var paddingWidth = Math.Abs(_alignment) - formatted.Length;

        if (_alignment > 0 && paddingWidth > 0)
        {
            builder.Append(' ', paddingWidth);
        }

        builder.Append(formatted);

        if (_alignment < 0 && paddingWidth > 0)
        {
            builder.Append(' ', paddingWidth);
        }
    }

    private static string FormatValue(string? format, object? argument, IFormatProvider formatProvider)
    {
        var formatter = (ICustomFormatter?)formatProvider.GetFormat(typeof(ICustomFormatter));
        if (formatter != null)
        {
            return formatter.Format(format, argument, formatProvider);
        }

        var formatted = argument switch
        {
            null => "<null>",
            IFormattable formattable => formattable.ToString(format, formatProvider),
            string str => str,
            IEnumerable enumerable => FormatEnumerable(enumerable, formatProvider),
            _ => ToStringOrDefault(argument, argument.GetType().ToString()),
        };

        if (argument is string or char)
        {
            formatted = Quote(formatted);
        }

        return formatted;
    }

    private static string FormatEnumerable(IEnumerable argument, IFormatProvider formatProvider)
    {
        var builder = new StringBuilder();
        builder.Append('[');

        foreach (var item in argument)
        {
            if (builder.Length > 1)
            {
                builder.Append(", ");
            }

            builder.Append(FormatValue(null, item, formatProvider));
        }

        builder.Append(']');
        return builder.ToString();
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Exceptions from the failed ToString() should not impede logging, for example")]
    private static string ToStringOrDefault(object argument, string defaultValue)
    {
        try
        {
            var result = argument.ToString();
            return !string.IsNullOrEmpty(result) ? result : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static string Quote(string value)
    {
        value = value
            .Replace(@"\", @"\\")
            .Replace("\"", "\\\"");

        return '"' + value + '"';
    }
}
