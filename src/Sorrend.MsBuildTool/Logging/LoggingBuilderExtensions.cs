using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Sorrend.MsBuildTool.Logging
{
    internal static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddMsBuildConsole(this ILoggingBuilder builder)
            => builder
                .AddConsoleFormatter<MsBuildConsoleFormatter, ConsoleFormatterOptions>()
                .AddConsole(options => options.FormatterName = MsBuildConsoleFormatter.FormatterName);
    }
}
