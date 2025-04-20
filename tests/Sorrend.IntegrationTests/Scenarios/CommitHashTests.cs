using System.Threading.Tasks;
using Sorrend.IntegrationTests.Tools;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.IntegrationTests.Tools.SystemUnderTest;
using Xunit;

namespace Sorrend.IntegrationTests.Scenarios
{
    public class CommitHashTests
    {
        private readonly ProjectFactory _projectFactory;
        private readonly GitRepositoryFactory _gitRepositoryFactory;
        private readonly HeadlessSut _headlessSut;

        public CommitHashTests(
            ProjectFactory projectFactory,
            GitRepositoryFactory gitRepositoryFactory,
            HeadlessSut headlessSut)
        {
            _projectFactory = projectFactory;
            _gitRepositoryFactory = gitRepositoryFactory;
            _headlessSut = headlessSut;
        }

        [Fact]
        public async Task IsIncludedAsPreReleaseIdentifier()
        {
            var project = await _projectFactory.CreateAsync();

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            var result = await _headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            var expectedPreReleaseIdentifier = "r" + commit.Hash.GetShortHash(12);
            VersionAssert.PreReleaseEndsWith(expectedPreReleaseIdentifier, result.InformationalVersion);
            VersionAssert.DoesNotHaveBuildMetadata(result.InformationalVersion);
        }

        [Fact]
        public async Task IsIncludedAsReleaseBuildMetadata()
        {
            var project = await _projectFactory.CreateAsync();

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            await repository.TagAsync("v1.0.0");

            var result = await _headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            VersionAssert.DoesNotHavePreRelease(result.InformationalVersion);
            VersionAssert.BuildMetadataEquals(commit.Hash.ToString(), result.InformationalVersion);
        }
    }
}
