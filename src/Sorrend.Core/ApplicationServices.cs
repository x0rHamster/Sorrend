using Sorrend.Core.AssemblyVersioning;
using Sorrend.Core.UserMessages;
using Sorrend.Core.VersionControl;

namespace Sorrend.Core;

public class ApplicationServices(
    RepositoryLocator repositoryLocator,
    AssemblyVersionCalculationFactory assemblyVersionCalculationFactory,
    AssemblyVersionSerializer assemblyVersionSerializer)
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

        var calculation = assemblyVersionCalculationFactory.Create();

        var repository = await repositoryLocator.GetAsync(projectDirectoryPath);
        var commits = await repository.GetFirstParentCommitsAsync();

        foreach (var commit in commits)
        {
            using var commitScope = UserMessageScopes.Commit(commit);

            calculation.Add(commit);

            if (calculation.HasResult)
            {
                break;
            }
        }

        var assemblyVersion = calculation.GetResult();

        var assemblyVersionBytes = assemblyVersionSerializer.Serialize(assemblyVersion);
        File.WriteAllBytes(assemblyVersionFilePath, assemblyVersionBytes);
    }
}
