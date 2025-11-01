using System.Diagnostics;
using System.Text;

namespace Sorrend.Core.OperatingSystem;

public class SystemCommand
{
    private string? _workingDirectoryPath;

    public SystemCommand WithWorkingDirectory(string directoryPath)
    {
        _workingDirectoryPath = directoryPath;
        return this;
    }

    [UsedImplicitly]
    [Obsolete("This method requires at least one argument.", true)]
    public Task<SystemCommandResult> RunAsync()
        => throw new NotSupportedException("This method requires at least one argument.");

    public Task<SystemCommandResult> RunAsync(params string[] arguments)
        => RunAsync((IEnumerable<string>)arguments);

    public async Task<SystemCommandResult> RunAsync(IEnumerable<string> arguments)
    {
        var result = await RunProcessAsync(arguments);
        ThrowIfNonZeroExitCode(result);
        return result;
    }

    private async Task<SystemCommandResult> RunProcessAsync(IEnumerable<string> arguments)
    {
        using var process = PrepareProcess(arguments);

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.EnableRaisingEvents = true;

        var exitTcs = new TaskCompletionSource<int>();
        process.Exited += (sender, _) => exitTcs.SetResult(((Process)sender).ExitCode);

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        return new SystemCommandResult(
            Path.GetFileName(process.StartInfo.FileName),
            await exitTcs.Task,
            await outputTask,
            await errorTask);
    }

    private Process PrepareProcess(IEnumerable<string> arguments)
    {
        var process = new Process
        {
            StartInfo =
            {
                WorkingDirectory = _workingDirectoryPath ?? string.Empty,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        var argumentsBuilder = new StringBuilder();

        foreach (var argument in arguments)
        {
            if (process.StartInfo.FileName == string.Empty)
            {
                process.StartInfo.FileName = argument;
                continue;
            }

            if (argumentsBuilder.Length > 0)
            {
                argumentsBuilder.Append(' ');
            }

            argumentsBuilder.Append('"');
            argumentsBuilder.Append(argument.Replace("\"", "\"\"\""));
            argumentsBuilder.Append('"');
        }

        process.StartInfo.Arguments = argumentsBuilder.ToString();

        return process;
    }

    private static void ThrowIfNonZeroExitCode(SystemCommandResult result)
    {
        if (result.ExitCode == 0)
        {
            return;
        }

        var messageBuilder = new StringBuilder(
            $"The command \"{result.CommandName}\" exited with a non-zero code {result.ExitCode}.");

        if (!string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            messageBuilder.AppendLine();
            messageBuilder.AppendLine();
            messageBuilder.Append(TrimOutput(result.StandardOutput));
        }

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            messageBuilder.AppendLine();
            messageBuilder.AppendLine();
            messageBuilder.Append(TrimOutput(result.StandardError));
        }

        throw new InvalidOperationException(messageBuilder.ToString());
    }

    private static string TrimOutput(string value)
        => value.TrimStart('\r', '\n').TrimEnd();
}
