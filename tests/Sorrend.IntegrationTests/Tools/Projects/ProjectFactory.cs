using System;
using System.IO;
using System.Threading.Tasks;
using Sorrend.IntegrationTests.Tools.Packages;

namespace Sorrend.IntegrationTests.Tools.Projects
{
    public class ProjectFactory
    {
        private readonly TestingEnvironment.Provider _testingEnvironmentProvider;
        private readonly PackageManager.Provider _packageManagerProvider;

        public ProjectFactory(
            TestingEnvironment.Provider testingEnvironmentProvider,
            PackageManager.Provider packageManagerProvider)
        {
            _testingEnvironmentProvider = testingEnvironmentProvider;
            _packageManagerProvider = packageManagerProvider;
        }

        public Task<ProjectDescription> CreateAsync(Action<ProjectSpecification> configure = null)
        {
            var specification = new ProjectSpecification();
            configure?.Invoke(specification);
            return CreateAsync(specification);
        }

        private async Task<ProjectDescription> CreateAsync(ProjectSpecification specification)
        {
            if (specification.TargetFrameworks.Count > 1 && !specification.EffectiveSdkStyle)
            {
                throw new InvalidOperationException(
                    "The project has multiple target frameworks, but it is not SDK-style and does not support multitargeting.");
            }

            var testingEnvironment = await _testingEnvironmentProvider.GetAsync();
            var packageManager = await _packageManagerProvider.GetAsync();

            var workingDirectoryPath = testingEnvironment.WorkingDirectoryPath;

            var projectDirectoryPath = Path.Combine(workingDirectoryPath, Generate.DirectoryName());
            Directory.CreateDirectory(projectDirectoryPath);

            var projectXml = ProjectTranslator.GetProjectXml(
                specification,
                packageManager.GlobalPackagesDirectoryPath);

            var projectFilePath = Path.Combine(projectDirectoryPath, "Project.csproj");
            projectXml.Save(projectFilePath);

            if (!specification.EffectiveSdkStyle)
            {
                var packagesConfigXml = ProjectTranslator.GetPackagesConfigXml(specification);
                var packagesConfigFilePath = Path.Combine(projectDirectoryPath, "packages.config");
                packagesConfigXml.Save(packagesConfigFilePath);
            }

            return new ProjectDescription(projectFilePath, specification);
        }
    }
}
