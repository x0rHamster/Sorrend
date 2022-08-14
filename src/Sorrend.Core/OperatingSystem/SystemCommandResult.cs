namespace Sorrend.Core.OperatingSystem
{
    public class SystemCommandResult
    {
        public string CommandName { get; }

        public int ExitCode { get; }

        public string StandardOutput { get; }

        public string StandardError { get; }

        public SystemCommandResult(
            string commandName,
            int exitCode,
            string standardOutput,
            string standardError)
        {
            CommandName = commandName;
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }
    }
}
