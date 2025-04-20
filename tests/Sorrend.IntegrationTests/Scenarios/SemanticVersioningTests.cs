using System.Threading.Tasks;
using Sorrend.IntegrationTests.Tools;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.IntegrationTests.Tools.SystemUnderTest;
using Xunit;

namespace Sorrend.IntegrationTests.Scenarios
{
    public class SemanticVersioningTests
    {
        private readonly PackageManager.Provider _packageManagerProvider;
        private readonly ProjectFactory _projectFactory;
        private readonly GitRepositoryFactory _gitRepositoryFactory;
        private readonly HeadlessSut _headlessSut;

        public SemanticVersioningTests(
            PackageManager.Provider packageManagerProvider,
            ProjectFactory projectFactory,
            GitRepositoryFactory gitRepositoryFactory,
            HeadlessSut headlessSut)
        {
            _packageManagerProvider = packageManagerProvider;
            _projectFactory = projectFactory;
            _gitRepositoryFactory = gitRepositoryFactory;
            _headlessSut = headlessSut;
        }

        [Fact]
        public async Task InitialCommit_IsZeroMajorPreRelease()
        {
            var packageManager = await _packageManagerProvider.GetAsync();

            var project = await _projectFactory.CreateAsync(
                x => x.WithReference(packageManager.PackageUnderTest));

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            await repository.CommitAsync();

            var result = await _headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            Assert.Equal("0.0.0.0", result.AssemblyVersion);
            Assert.Equal("0.1.0.0", result.FileVersion);
            VersionAssert.NormalVersionEquals("0.1.0", result.InformationalVersion);
            VersionAssert.PreReleaseStartsWith("dev.1", result.InformationalVersion);
        }

        [Fact]
        public async Task Commit_IncrementsPreReleaseCounter()
        {
            var project = await _projectFactory.CreateAsync();

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            await repository.CommitAsync();

            await _projectFactory.UpdateAsync(project);
            await repository.CommitAsync();

            await _projectFactory.UpdateAsync(project);
            await repository.CommitAsync();

            var result = await _headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            Assert.Equal("0.0.0.0", result.AssemblyVersion);
            Assert.Equal("0.1.0.0", result.FileVersion);
            VersionAssert.NormalVersionEquals("0.1.0", result.InformationalVersion);
            VersionAssert.PreReleaseStartsWith("dev.3", result.InformationalVersion);
        }

        [Fact]
        public async Task MergeCommit_IncrementsPreReleaseCounter_FromBaseBranch()
        {
            var project = await _projectFactory.CreateAsync();

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            await repository.CommitAsync();

            await repository.CreateBranchAsync("develop");
            await repository.CreateBranchAsync("feature");

            await repository.CheckoutAsync("feature");

            await _projectFactory.UpdateAsync(project);
            await repository.CommitAsync();

            await _projectFactory.UpdateAsync(project);
            await repository.CommitAsync();

            await repository.CheckoutAsync("develop");

            await repository.MergeAsync("feature");

            var result = await _headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            VersionAssert.PreReleaseStartsWith("dev.2", result.InformationalVersion);
        }

        [Fact]
        public async Task TaggedCommit_IsRelease()
        {
            var project = await _projectFactory.CreateAsync();

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            await repository.CommitAsync();

            await repository.TagAsync("v2.3.4");

            var result = await _headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            Assert.Equal("2.0.0.0", result.AssemblyVersion);
            Assert.Equal("2.3.4.0", result.FileVersion);
            VersionAssert.NormalVersionEquals("2.3.4", result.InformationalVersion);
            VersionAssert.DoesNotHavePreRelease(result.InformationalVersion);
        }

        [Fact]
        public async Task CommitAfterTagged_IsPatchPreRelease()
        {
            var project = await _projectFactory.CreateAsync();

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            await repository.CommitAsync();

            await repository.TagAsync("v2.3.4");

            await _projectFactory.UpdateAsync(project);
            await repository.CommitAsync();

            var result = await _headlessSut.CalculateAssemblyVersionAsync(project.FilePath);

            Assert.Equal("2.0.0.0", result.AssemblyVersion);
            Assert.Equal("2.3.5.0", result.FileVersion);
            VersionAssert.NormalVersionEquals("2.3.5", result.InformationalVersion);
            VersionAssert.PreReleaseStartsWith("dev.1", result.InformationalVersion);
        }
    }
}
