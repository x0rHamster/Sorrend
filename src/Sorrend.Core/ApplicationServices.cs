using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.Core
{
    public class ApplicationServices
    {
        public async Task CalculateAssemblyVersionAsync(
            string workingDirectoryPath,
            string projectFilePathRelativeToWorkingDirectory,
            string assemblyVersionFilePathRelativeToProject)
        {
            var projectFilePath = Path.GetFullPath(
                Path.Combine(
                    workingDirectoryPath,
                    projectFilePathRelativeToWorkingDirectory));

            var projectDirectoryPath = Path.GetDirectoryName(projectFilePath)
                ?? throw new ArgumentException(
                    $"Path \"{projectFilePath}\" does not have a directory.",
                    nameof(projectFilePathRelativeToWorkingDirectory));

            var assemblyVersionFilePath = Path.GetFullPath(
                Path.Combine(
                    projectDirectoryPath,
                    assemblyVersionFilePathRelativeToProject));

            var projectCommitShortHash = await GetHeadCommitShortHashAsync(projectDirectoryPath);

            WriteAssemblyVersionFile(assemblyVersionFilePath, $"0.1.0+{projectCommitShortHash}");
        }

        private static async Task<string> GetHeadCommitShortHashAsync(string directoryPath)
        {
            var result = await new SystemCommand()
                .WithWorkingDirectory(directoryPath)
                .RunAsync("git", "rev-parse", "--short", "HEAD");

            return result.StandardOutput.TrimEnd('\n');
        }

        private static void WriteAssemblyVersionFile(string assemblyVersionFilePath, string informationalVersion)
        {
            XNamespace ns = "urn:sorrend:v0";

            var document = new XDocument(
                new XDeclaration(null, null, null),
                new XElement(
                    ns + "AssemblyVersion",
                    new XElement(
                        ns + "ProjectProperties",
                        new XElement(
                            ns + "ProjectProperty",
                            new XAttribute("Name", "InformationalVersion"),
                            new XAttribute("Value", informationalVersion)),
                        new XElement(
                            ns + "ProjectProperty",
                            new XAttribute("Name", "PackageVersion"),
                            new XAttribute("Value", informationalVersion)),
                        new XElement(
                            ns + "ProjectProperty",
                            new XAttribute("Name", "GenerateAssemblyInformationalVersionAttribute"),
                            new XAttribute("Value", "false")),
                        new XElement(
                            ns + "ProjectProperty",
                            new XAttribute("Name", "IncludeSourceRevisionInInformationalVersion"),
                            new XAttribute("Value", "false"))),
                    new XElement(
                        ns + "AssemblyAttributes",
                        new XElement(
                            ns + "AssemblyAttribute",
                            new XAttribute("TypeName", "System.Reflection.AssemblyInformationalVersionAttribute"),
                            new XAttribute("Parameter1", informationalVersion)))));

            document.Save(assemblyVersionFilePath);
        }
    }
}
