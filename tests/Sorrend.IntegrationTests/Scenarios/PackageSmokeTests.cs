using System.Threading.Tasks;
using Sorrend.IntegrationTests.Tools.Assemblies;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Xunit;

namespace Sorrend.IntegrationTests.Scenarios
{
    public class PackageSmokeTests
    {
        private readonly PackageManager.Provider _packageManagerProvider;
        private readonly ProjectFactory _projectFactory;
        private readonly BuildSystem.Provider _buildSystemProvider;
        private readonly GitRepositoryFactory _gitRepositoryFactory;
        private readonly AssemblyAnalyzer _assemblyAnalyzer;

        public PackageSmokeTests(
            PackageManager.Provider packageManagerProvider,
            ProjectFactory projectFactory,
            BuildSystem.Provider buildSystemProvider,
            GitRepositoryFactory gitRepositoryFactory,
            AssemblyAnalyzer assemblyAnalyzer)
        {
            _packageManagerProvider = packageManagerProvider;
            _projectFactory = projectFactory;
            _buildSystemProvider = buildSystemProvider;
            _gitRepositoryFactory = gitRepositoryFactory;
            _assemblyAnalyzer = assemblyAnalyzer;
        }

        [Fact]
        public async Task AffectsAssemblyVersion()
        {
            var packageManager = await _packageManagerProvider.GetAsync();
            var buildSystem = await _buildSystemProvider.GetAsync();

            var project = await _projectFactory.CreateAsync(
                x => x.WithReference(packageManager.PackageUnderTest));

            var repository = await _gitRepositoryFactory.CreateAsync(project.DirectoryPath);
            var commit = await repository.CommitAsync();

            await buildSystem.BuildAsync(project);

            var assembly = await _assemblyAnalyzer.LoadAsync(project.AssemblyFilePath);
            Assert.Contains(commit.ShortHash, assembly.InformationalVersion);
        }
    }
}
