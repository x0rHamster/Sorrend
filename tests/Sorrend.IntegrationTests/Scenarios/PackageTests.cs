using Sorrend.IntegrationTests.Tools.Assemblies;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;

namespace Sorrend.IntegrationTests.Scenarios;

public class PackageTests(
    PackageManager.Provider packageManagerProvider,
    ProjectFactory projectFactory,
    BuildSystem.Provider buildSystemProvider,
    GitRepositoryFactory gitRepositoryFactory,
    AssemblyAnalyzer assemblyAnalyzer,
    PackageAnalyzer packageAnalyzer)
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SupportsSdkStyleProjects(bool sdkStyle, bool msBuildCore)
    {
        var packageManager = await packageManagerProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            specification => specification
                .WithSdkStyle(sdkStyle)
                .WithReference(packageManager.PackageUnderTest));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        var commit = await repository.CommitAsync();

        await (msBuildCore
            ? buildSystem.BuildUsingDotnetBuildAsync(project)
            : buildSystem.BuildUsingMsBuildAsync(project));

        var assembly = await assemblyAnalyzer.LoadAsync(project.AssemblyFilePath);
        Assert.Contains(commit.ShortHash, assembly.InformationalVersion);
    }

    [Fact]
    public async Task SupportsMultiTargetProjects()
    {
        var packageManager = await packageManagerProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            specification => specification
                .WithTargetFrameworks(TargetFramework.NetFramework472, TargetFramework.Net10)
                .WithReference(packageManager.PackageUnderTest));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        var commit = await repository.CommitAsync();

        await buildSystem.BuildAsync(project);

        // It is difficult to run the task once for all TFMs (see dotnet/msbuild#2781). At the same time, source
        // generators (Roslyn, Uno) are run for each TFM separately. For these reasons, we accept the risk that
        // restarting the task with a potentially sequential build for different TFMs will slow down the overall
        // build process. That is why we do not check the number of task runs

        foreach (var assemblyFilePath in project.AssemblyFilePaths)
        {
            var assembly = await assemblyAnalyzer.LoadAsync(assemblyFilePath);
            Assert.Contains(commit.ShortHash, assembly.InformationalVersion);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AffectsPackageVersion(bool sdkStyle)
    {
        var packageManager = await packageManagerProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            specification => specification
                .WithSdkStyle(sdkStyle)
                .WithReference(packageManager.PackageUnderTest));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        var commit = await repository.CommitAsync();

        await buildSystem.BuildAsync(project);
        await buildSystem.PackAsync(project);

        var package = await packageAnalyzer.LoadAsync(project.PackageFilePath);
        Assert.Contains(commit.ShortHash, package.Version);
        Assert.DoesNotContain(packageManager.PackageUnderTest.Id, package.DependencyPackageIds);
    }
}
