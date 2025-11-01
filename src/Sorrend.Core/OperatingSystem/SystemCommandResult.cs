namespace Sorrend.Core.OperatingSystem;

public record SystemCommandResult(
    string CommandName,
    int ExitCode,
    string StandardOutput,
    string StandardError);
