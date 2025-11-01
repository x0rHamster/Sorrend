using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Sorrend.Core.Utilities.MessageTemplates;

namespace Sorrend.MsBuildTool.Logging;

internal class MsBuildConsoleFormatter() : ConsoleFormatter(FormatterName)
{
    public const string FormatterName = "msbuild";

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        textWriter.Write(nameof(Sorrend));
        textWriter.Write(" : ");
        textWriter.Write(FormatLogLevel(logEntry.LogLevel));
        textWriter.Write(" : ");
        textWriter.Write(FormatMessage(logEntry, textWriter.FormatProvider));

        if (logEntry.Exception != null)
        {
            textWriter.Write(" ");
            textWriter.Write(logEntry.Exception.ToString());
        }

        textWriter.WriteLine();
    }

    private static string FormatLogLevel(LogLevel logLevel)
        => logLevel >= LogLevel.Error ? "ERROR"
            : logLevel == LogLevel.Warning ? "Warning"
            : "Message";

    private static string FormatMessage<TState>(
        LogEntry<TState> logEntry,
        IFormatProvider formatProvider)
    {
        if (
            logEntry.State is IReadOnlyList<KeyValuePair<string, object?>> items
            && items[^1].Value is string messageTemplate)
        {
            var arguments = items
                .Take(items.Count - 1)
                .Select(x => x.Value)
                .ToArray();

            return MessageFormatter.Format(
                messageTemplate,
                arguments,
                formatProvider);
        }

        return logEntry.Formatter(logEntry.State, logEntry.Exception);
    }
}
