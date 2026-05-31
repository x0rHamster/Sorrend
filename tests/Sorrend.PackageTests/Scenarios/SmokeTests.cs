using Sorrend.IntegrationTests.Tools.Assemblies;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.PackageTests.Tools;

namespace Sorrend.PackageTests.Scenarios;

public class SmokeTests(
    PackageUnderTestProvider packageUnderTestProvider,
    ProjectFactory projectFactory,
    BuildSystem.Provider buildSystemProvider,
    GitRepositoryFactory gitRepositoryFactory,
    AssemblyAnalyzer assemblyAnalyzer)
{
    [Fact]
    public async Task AffectsAssemblyVersion()
    {
        var packageUnderTest = await packageUnderTestProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            x => x.WithReference(packageUnderTest));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        var commit = await repository.CommitAsync();

        await buildSystem.BuildAsync(project);

        var assembly = await assemblyAnalyzer.LoadAsync(project.AssemblyFilePath);
        Assert.Contains(commit.ShortHash, assembly.InformationalVersion);
    }
}
