using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Sorrend.Core.UserMessages;

namespace Sorrend.MsBuildTool.Logging
{
    internal static class LoggerExtensions
    {
        public static void LogApplicationCrash(this ILogger logger, Exception exception)
            => logger.LogCritical(exception, "The application crashed due to an unhandled exception.");

        [SuppressMessage(
            "Usage",
            "CA2254:Template should be a static expression",
            Justification = "The properties used conform to the Message Templates specification")]
        public static void LogUserOrientedError(this ILogger logger, UserOrientedException exception)
            => logger.LogError(exception.MessageTemplate, exception.Arguments);
    }
}
