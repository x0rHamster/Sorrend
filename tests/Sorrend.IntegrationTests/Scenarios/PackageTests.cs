using System.Threading.Tasks;
using Sorrend.IntegrationTests.Tools.Assemblies;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Xunit;

namespace Sorrend.IntegrationTests.Scenarios
{
    public class PackageTests
    {
        private readonly PackageManager.Provider _packageManagerProvider;
        private readonly ProjectFactory _projectFactory;
        private readonly BuildSystem.Provider _buildSystemProvider;
        private readonly GitRepositoryFactory _gitRepositoryFactory;
        private readonly AssemblyAnalyzer _assemblyAnalyzer;
        private readonly PackageAnalyzer _packageAnalyzer;

        public PackageTests(
            PackageManager.Provider packageManagerProvider,
            ProjectFactory projectFactory,
            BuildSystem.Provider buildSystemProvider,
            GitRepositoryFactory gitRepositoryFactory,
            AssemblyAnalyzer assemblyAnalyzer,
            PackageAnalyzer packageAnalyzer)
        {
            _packageManagerProvider = packageManagerProvider;
            _projectFactory = projectFactory;
            _buildSystemProvider = buildSystemProvider;
            _gitRepositoryFactory = gitRepositoryFactory;
            _assemblyAnalyzer = assemblyAnalyzer;
            _packageAnalyzer = packageAnalyzer;
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task SupportsSdkStyleProjects(bool sdkStyle, bool msBuildCore)
        {
            var packageManager = await _packageManagerProvider.GetAsync();
            var buildSystem = await _buildSystemProvider.GetAsync();

            var project = await _projectFactory.CreateAsync(
                specification => specification
                    .WithSdkStyle(sdkStyle)
                    .WithReference(packageManager.PackageUnderTest));

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            await (msBuildCore
                ? buildSystem.BuildUsingDotnetBuildAsync(project)
                : buildSystem.BuildUsingMsBuildAsync(project));

            var assembly = await _assemblyAnalyzer.LoadAsync(project.AssemblyFilePath);
            Assert.Contains(commit.ShortHash, assembly.InformationalVersion);
        }

        [Fact]
        public async Task SupportsMultiTargetProjects()
        {
            var packageManager = await _packageManagerProvider.GetAsync();
            var buildSystem = await _buildSystemProvider.GetAsync();

            var project = await _projectFactory.CreateAsync(
                specification => specification
                    .WithTargetFrameworks(TargetFramework.NetFramework472, TargetFramework.Net6)
                    .WithReference(packageManager.PackageUnderTest));

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            await buildSystem.BuildAsync(project);

            // It is difficult to run the task once for all TFMs (see dotnet/msbuild#2781). At the same time, source
            // generators (Roslyn, Uno) are run for each TFM separately. For these reasons, we accept the risk that
            // restarting the task with a potentially sequential build for different TFMs will slow down the overall
            // build process. That is why we do not check the number of task runs

            foreach (var assemblyFilePath in project.AssemblyFilePaths)
            {
                var assembly = await _assemblyAnalyzer.LoadAsync(assemblyFilePath);
                Assert.Contains(commit.ShortHash, assembly.InformationalVersion);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AffectsPackageVersion(bool sdkStyle)
        {
            var packageManager = await _packageManagerProvider.GetAsync();
            var buildSystem = await _buildSystemProvider.GetAsync();

            var project = await _projectFactory.CreateAsync(
                specification => specification
                    .WithSdkStyle(sdkStyle)
                    .WithReference(packageManager.PackageUnderTest));

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            await buildSystem.BuildAsync(project);
            await buildSystem.PackAsync(project);

            var package = await _packageAnalyzer.LoadAsync(project.PackageFilePath);
            Assert.Contains(commit.ShortHash, package.Version);
            Assert.DoesNotContain(packageManager.PackageUnderTest.Id, package.DependencyPackageIds);
        }
    }
}
