using Sorrend.Core.VersioningSchemes;
using Sorrend.IntegrationTests.Tools;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.IntegrationTests.Tools.SystemUnderTest;

namespace Sorrend.IntegrationTests.Scenarios;

public class CalendarVersioningTests(
    ProjectFactory projectFactory,
    GitRepositoryFactory gitRepositoryFactory,
    HeadlessSut headlessSut,
    SutConfigurationWriter sutConfigurationWriter)
{
    [Fact]
    public async Task Commit_HasPreReleaseVersion_BasedOnDate()
    {
        var project = await projectFactory.CreateAsync();

        await sutConfigurationWriter.WriteAsync(
            specification => specification
                .WithFileDirectory(project.DirectoryPath)
                .WithVersioningScheme(VersioningSchemeIdentifier.CalendarVersioning));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        await repository.CommitAsync(x => x.WithAuthorDateTime("1999-12-31T23:59:59Z"));

        await projectFactory.UpdateAsync(project);
        await repository.CommitAsync(x => x.WithAuthorDateTime("2012-08-06T05:17:57Z"));

        var result = await headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

        Assert.Equal("1.0.0.0", result.AssemblyVersion);
        Assert.Equal("2012.31423.0.0", result.FileVersion);
        VersionAssert.NormalVersionEquals("2012.31423.0", result.InformationalVersion);
        VersionAssert.HasPreRelease(result.InformationalVersion);
    }

    [Fact]
    public async Task TaggedCommit_HasVersion_FromTag()
    {
        var project = await projectFactory.CreateAsync();

        await sutConfigurationWriter.WriteAsync(
            specification => specification
                .WithFileDirectory(project.DirectoryPath)
                .WithVersioningScheme(VersioningSchemeIdentifier.CalendarVersioning));

        var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
        await repository.CommitAsync(x => x.WithAuthorDateTime("2012-08-06T05:17:57Z"));
        await repository.TagAsync("v2011.47466.1");

        var result = await headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

        Assert.Equal("1.0.0.0", result.AssemblyVersion);
        Assert.Equal("2011.47466.1.0", result.FileVersion);
        VersionAssert.NormalVersionEquals("2011.47466.1", result.InformationalVersion);
        VersionAssert.DoesNotHavePreRelease(result.InformationalVersion);
    }
}
