using Sorrend.IntegrationTests.Tools.Assemblies;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.PackageTests.Tools;

namespace Sorrend.PackageTests.Scenarios;

public class ProjectCompatibilityTests(
    PackageUnderTestProvider packageUnderTestProvider,
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
        var packageUnderTest = await packageUnderTestProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            specification => specification
                .WithSdkStyle(sdkStyle)
                .WithReference(packageUnderTest));

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
        var packageUnderTest = await packageUnderTestProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            specification => specification
                .WithTargetFrameworks(TargetFramework.NetFramework472, TargetFramework.Net10)
                .WithReference(packageUnderTest));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        var commit = await repository.CommitAsync();

        await buildSystem.BuildAsync(project);

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
        var packageUnderTest = await packageUnderTestProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            specification => specification
                .WithSdkStyle(sdkStyle)
                .WithReference(packageUnderTest));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        var commit = await repository.CommitAsync();

        await buildSystem.BuildAsync(project);
        await buildSystem.PackAsync(project);

        var package = await packageAnalyzer.LoadAsync(project.PackageFilePath);
        Assert.Contains(commit.ShortHash, package.Version);
        Assert.DoesNotContain(packageUnderTest.Id, package.DependencyPackageIds);
    }

    [Fact]
    public async Task AffectsPackageVersion_ForMultiTargetProjects()
    {
        var packageUnderTest = await packageUnderTestProvider.GetAsync();
        var buildSystem = await buildSystemProvider.GetAsync();

        var project = await projectFactory.CreateAsync(
            specification => specification
                .WithTargetFrameworks(TargetFramework.NetFramework472, TargetFramework.Net10)
                .WithReference(packageUnderTest));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        var commit = await repository.CommitAsync();

        await buildSystem.BuildAsync(project);
        await buildSystem.PackAsync(project);

        var package = await packageAnalyzer.LoadAsync(project.PackageFilePath);
        Assert.Contains(commit.ShortHash, package.Version);
    }
}
