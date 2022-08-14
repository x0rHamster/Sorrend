using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sorrend.Core;
using Sorrend.MsBuildTool.Logging;

namespace Sorrend.MsBuildTool
{
    internal static class Program
    {
        [SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "Logs an unhandled exception before the application terminates")]
        private static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddLogging(x => x.AddMsBuildConsole());
            services.AddSorrendCore();

            using (var provider = services.BuildServiceProvider())
            {
                var logger = provider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(nameof(Program));

                try
                {
                    var args = Environment.GetCommandLineArgs();
                    var projectFilePath = args[1];
                    var assemblyVersionFilePath = args[2];

                    await provider
                        .GetRequiredService<ApplicationServices>()
                        .CalculateAssemblyVersionAsync(
                            Directory.GetCurrentDirectory(),
                            projectFilePath,
                            assemblyVersionFilePath);
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "The application crashed due to an unhandled exception.");
                    Environment.ExitCode = -1;
                }
            }
        }
    }
}
