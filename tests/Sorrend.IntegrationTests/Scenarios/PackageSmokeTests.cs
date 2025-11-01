using System.Threading.Tasks;
using Sorrend.IntegrationTests.Tools.Assemblies;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Xunit;

namespace Sorrend.IntegrationTests.Scenarios
{
    public class PackageSmokeTests(
        PackageManager.Provider packageManagerProvider,
        ProjectFactory projectFactory,
        BuildSystem.Provider buildSystemProvider,
        GitRepositoryFactory gitRepositoryFactory,
        AssemblyAnalyzer assemblyAnalyzer)
    {
        [Fact]
        public async Task AffectsAssemblyVersion()
        {
            var packageManager = await packageManagerProvider.GetAsync();
            var buildSystem = await buildSystemProvider.GetAsync();

            var project = await projectFactory.CreateAsync(
                x => x.WithReference(packageManager.PackageUnderTest));

            var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            await buildSystem.BuildAsync(project);

            var assembly = await assemblyAnalyzer.LoadAsync(project.AssemblyFilePath);
            Assert.Contains(commit.ShortHash, assembly.InformationalVersion);
        }
    }
}
