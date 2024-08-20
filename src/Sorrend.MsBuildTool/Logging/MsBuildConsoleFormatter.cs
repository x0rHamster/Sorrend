using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Sorrend.MsBuildTool.Logging
{
    internal class MsBuildConsoleFormatter : ConsoleFormatter
    {
        public const string FormatterName = "msbuild";

        public MsBuildConsoleFormatter()
            : base(FormatterName)
        {
        }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (message == null)
            {
                return;
            }

            textWriter.Write(nameof(Sorrend));
            textWriter.Write(" : ");
            textWriter.Write(FormatLogLevel(logEntry.LogLevel));
            textWriter.Write(" : ");
            textWriter.Write(message);

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
    }
}
