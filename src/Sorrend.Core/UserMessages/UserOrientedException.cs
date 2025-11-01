using System.Globalization;
using Sorrend.Core.Utilities.MessageTemplates;

namespace Sorrend.Core.UserMessages;

[SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification = "The inner exception argument conflicts with the Message Templates arguments")]
public class UserOrientedException : Exception
{
    public string MessageTemplate { get; } = string.Empty;

    public IReadOnlyList<object?> Arguments { get; } = [];

    public IFormatProvider FormatProvider { get; } = CultureInfo.CurrentCulture;

    public UserOrientedException()
    {
    }

    public UserOrientedException(
        IFormatProvider formatProvider,
        string messageTemplate,
        params object?[] arguments)
        : base(FormatMessage(messageTemplate, arguments, formatProvider))
    {
        MessageTemplate = messageTemplate;
        Arguments = arguments;
        FormatProvider = formatProvider;
    }

    public UserOrientedException(
        Exception innerException,
        IFormatProvider formatProvider,
        string messageTemplate,
        params object?[] arguments)
        : base(
            FormatMessage(messageTemplate, arguments, formatProvider),
            innerException)
    {
        MessageTemplate = messageTemplate;
        Arguments = arguments;
        FormatProvider = formatProvider;
    }

    private static string FormatMessage(
        string messageTemplate,
        object?[] arguments,
        IFormatProvider formatProvider)
    {
        return MessageFormatter.Format(
            messageTemplate,
            arguments,
            formatProvider);
    }
}
