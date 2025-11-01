using Sorrend.Core.OperatingSystem;
using Sorrend.IntegrationTests.Utilities;

namespace Sorrend.IntegrationTests.Tools.Packages;

public class PackageManager
{
    private readonly PackageAnalyzer _packageAnalyzer;
    private readonly string _packageSourceDirectoryPath;

    private PackageDescription? _initializedPackageUnderTest;

    public string GlobalPackagesDirectoryPath { get; }

    public PackageDescription PackageUnderTest
        => _initializedPackageUnderTest
            ?? throw new InvalidOperationException(
                $"The object of type \"{typeof(PackageDescription)}\" has not been initialized.");

    private PackageManager(
        PackageAnalyzer packageAnalyzer,
        string packageSourceDirectoryPath,
        string globalPackagesDirectoryPath)
    {
        _packageAnalyzer = packageAnalyzer;
        _packageSourceDirectoryPath = packageSourceDirectoryPath;
        GlobalPackagesDirectoryPath = globalPackagesDirectoryPath;
    }

    private async Task<PackageDescription> PushPackageAsync(string filePath)
    {
        await new SystemCommand().RunAsync(
            "dotnet",
            "nuget",
            "push",
            filePath,
            "--source",
            _packageSourceDirectoryPath);

        return await _packageAnalyzer.LoadAsync(filePath);
    }

    public class Provider(
        TestingEnvironment.Provider testingEnvironmentProvider,
        PackageAnalyzer packageAnalyzer)
        : AsyncInitializingProvider<PackageManager>
    {
        protected override async Task<PackageManager> CreateInitializedAsync()
        {
            var testingEnvironment = await testingEnvironmentProvider.GetAsync();

            var workingDirectoryPath = testingEnvironment.WorkingDirectoryPath;
            var packageSourceDirectoryPath = Path.Combine(workingDirectoryPath, "PackageSource");
            var globalPackagesDirectoryPath = Path.Combine(workingDirectoryPath, "GlobalPackages");

            await InitializeEnvironmentAsync(
                workingDirectoryPath,
                packageSourceDirectoryPath,
                globalPackagesDirectoryPath);

            var instance = new PackageManager(
                packageAnalyzer,
                packageSourceDirectoryPath,
                globalPackagesDirectoryPath);

            instance._initializedPackageUnderTest =
                await instance.PushPackageAsync(testingEnvironment.PackageUnderTestFilePath);

            return instance;
        }

        private static Task InitializeEnvironmentAsync(
            string workingDirectoryPath,
            string packageSourceDirectoryPath,
            string globalPackagesDirectoryPath)
        {
            Directory.CreateDirectory(packageSourceDirectoryPath);
            Directory.CreateDirectory(globalPackagesDirectoryPath);

            var nugetConfigXml = PackageTranslator.GetNugetConfigXml(
                packageSourceDirectoryPath,
                globalPackagesDirectoryPath);

            var nugetConfigFilePath = Path.Combine(workingDirectoryPath, "nuget.config");
            nugetConfigXml.Save(nugetConfigFilePath);

            return Task.CompletedTask;
        }
    }
}
