using System.IO;
using Sorrend.Core.OperatingSystem;
using Sorrend.IntegrationTests.Tools.Utilities;

namespace Sorrend.IntegrationTests.Tools.Packages;

public class PackageManager
{
    private readonly AbsolutePath _packageSourceDirectoryPath;

    public AbsolutePath GlobalPackagesDirectoryPath { get; }

    private PackageManager(
        AbsolutePath packageSourceDirectoryPath,
        AbsolutePath globalPackagesDirectoryPath)
    {
        _packageSourceDirectoryPath = packageSourceDirectoryPath;
        GlobalPackagesDirectoryPath = globalPackagesDirectoryPath;
    }

    public async Task PublishPackageLocallyAsync(AbsolutePath filePath)
    {
        await new SystemCommand().RunAsync(
            "dotnet",
            "nuget",
            "push",
            filePath.ToString(),
            "--source",
            _packageSourceDirectoryPath.ToString());
    }

    public class Provider(TestingEnvironment.Provider testingEnvironmentProvider)
        : AsyncInitializingProvider<PackageManager>
    {
        protected override async Task<PackageManager> CreateInitializedAsync()
        {
            var testingEnvironment = await testingEnvironmentProvider.GetAsync();

            var workingDirectoryPath = testingEnvironment.WorkingDirectoryPath;
            var packageSourceDirectoryPath = workingDirectoryPath / "PackageSource";
            var globalPackagesDirectoryPath = workingDirectoryPath / "GlobalPackages";

            await InitializeEnvironmentAsync(
                workingDirectoryPath,
                packageSourceDirectoryPath,
                globalPackagesDirectoryPath);

            return new PackageManager(
                packageSourceDirectoryPath,
                globalPackagesDirectoryPath);
        }

        private static Task InitializeEnvironmentAsync(
            AbsolutePath workingDirectoryPath,
            AbsolutePath packageSourceDirectoryPath,
            AbsolutePath globalPackagesDirectoryPath)
        {
            Directory.CreateDirectory(packageSourceDirectoryPath.ToString());
            Directory.CreateDirectory(globalPackagesDirectoryPath.ToString());

            var nugetConfigXml = PackageTranslator.GetNugetConfigXml(
                packageSourceDirectoryPath,
                globalPackagesDirectoryPath);

            var nugetConfigFilePath = workingDirectoryPath / "nuget.config";
            nugetConfigXml.Save(nugetConfigFilePath.ToString());

            return Task.CompletedTask;
        }
    }
}
