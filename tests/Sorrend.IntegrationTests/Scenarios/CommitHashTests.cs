using System.Threading.Tasks;
using Sorrend.IntegrationTests.Tools;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.IntegrationTests.Tools.SystemUnderTest;
using Xunit;

namespace Sorrend.IntegrationTests.Scenarios
{
    public class CommitHashTests(
        ProjectFactory projectFactory,
        GitRepositoryFactory gitRepositoryFactory,
        HeadlessSut headlessSut)
    {
        [Fact]
        public async Task IsIncludedAsPreReleaseIdentifier()
        {
            var project = await projectFactory.CreateAsync();

            var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            var result = await headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            var expectedPreReleaseIdentifier = "r" + commit.Hash.GetShortHash(12);
            VersionAssert.PreReleaseEndsWith(expectedPreReleaseIdentifier, result.InformationalVersion);
            VersionAssert.DoesNotHaveBuildMetadata(result.InformationalVersion);
        }

        [Fact]
        public async Task IsIncludedAsReleaseBuildMetadata()
        {
            var project = await projectFactory.CreateAsync();

            var repository = await gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            await repository.TagAsync("v1.0.0");

            var result = await headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            VersionAssert.DoesNotHavePreRelease(result.InformationalVersion);
            VersionAssert.BuildMetadataEquals(commit.Hash.ToString(), result.InformationalVersion);
        }
    }
}
