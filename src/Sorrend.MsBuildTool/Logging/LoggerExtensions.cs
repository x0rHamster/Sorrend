using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Sorrend.Core;

namespace Sorrend.MsBuildTool.Logging
{
    internal static class LoggerExtensions
    {
        public static void LogApplicationCrash(this ILogger logger, Exception exception)
            => logger.LogCritical(exception, "The application crashed due to an unhandled exception.");

        [SuppressMessage(
            "Usage",
            "CA2254:Template should be a static expression",
            Justification = "The MSBuild log is used to show arbitrary messages to the user")]
        public static void LogUserOrientedError(this ILogger logger, UserOrientedException exception)
            => logger.LogError(exception.Message);
    }
}
