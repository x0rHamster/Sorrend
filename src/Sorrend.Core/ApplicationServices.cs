using System;
using System.IO;
using System.Threading.Tasks;
using Sorrend.Core.AssemblyVersioning;
using Sorrend.Core.VersionControl;

namespace Sorrend.Core
{
    public class ApplicationServices
    {
        private readonly RepositoryLocator _repositoryLocator;
        private readonly AssemblyVersionCalculationFactory _assemblyVersionCalculationFactory;
        private readonly AssemblyVersionSerializer _assemblyVersionSerializer;

        public ApplicationServices(
            RepositoryLocator repositoryLocator,
            AssemblyVersionCalculationFactory assemblyVersionCalculationFactory,
            AssemblyVersionSerializer assemblyVersionSerializer)
        {
            _repositoryLocator = repositoryLocator;
            _assemblyVersionCalculationFactory = assemblyVersionCalculationFactory;
            _assemblyVersionSerializer = assemblyVersionSerializer;
        }

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

            var calculation = _assemblyVersionCalculationFactory.Create();

            var repository = await _repositoryLocator.GetAsync(projectDirectoryPath);
            var commits = await repository.GetFirstParentCommitsAsync();

            foreach (var commit in commits)
            {
                calculation.Add(commit);

                if (calculation.HasResult)
                {
                    break;
                }
            }

            var assemblyVersion = calculation.GetResult();

            var assemblyVersionBytes = _assemblyVersionSerializer.Serialize(assemblyVersion);
            File.WriteAllBytes(assemblyVersionFilePath, assemblyVersionBytes);
        }
    }
}
