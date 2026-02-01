using System.IO;
using Sorrend.Core.AssemblyVersioning;
using Sorrend.Core.Configuration;
using Sorrend.Core.OperatingSystem;
using Sorrend.Core.UserMessages;
using Sorrend.Core.VersionControl;

namespace Sorrend.Core;

public class ApplicationServices(
    ConfigurationReaderFactory configurationReaderFactory,
    RepositoryLocator repositoryLocator,
    AssemblyVersionCalculationFactory assemblyVersionCalculationFactory,
    AssemblyVersionSerializer assemblyVersionSerializer)
{
    public async Task CalculateAssemblyVersionAsync(
        AbsolutePath projectFilePath,
        AbsolutePath assemblyVersionFilePath)
    {
        var projectDirectoryPath = projectFilePath.ParentDirectory;

        var configurationReader = configurationReaderFactory.Create();
        await configurationReader.ReadWorkingCopyAsync(projectDirectoryPath);
        var configuration = configurationReader.GetCurrentConfiguration();

        var calculation = assemblyVersionCalculationFactory.Create();

        var repository = await repositoryLocator.GetAsync(projectDirectoryPath);
        var commits = await repository.GetFirstParentCommitsAsync();

        foreach (var commit in commits)
        {
            using var commitScope = UserMessageScopes.Commit(commit);

            calculation.Add(commit, configuration.AssemblyVersioning);

            if (calculation.HasResult)
            {
                break;
            }
        }

        var assemblyVersion = calculation.GetResult(configuration.AssemblyVersioning);

        var assemblyVersionBytes = assemblyVersionSerializer.Serialize(assemblyVersion);
        File.WriteAllBytes(assemblyVersionFilePath.ToString(), assemblyVersionBytes);
    }
}
