using System.IO;
using Sorrend.Core.OperatingSystem;
using Sorrend.IntegrationTests.Utilities;

namespace Sorrend.IntegrationTests.Tools.Packages;

public class PackageManager
{
    private readonly PackageAnalyzer _packageAnalyzer;
    private readonly AbsolutePath _packageSourceDirectoryPath;

    private PackageDescription? _initializedPackageUnderTest;

    public AbsolutePath GlobalPackagesDirectoryPath { get; }

    public PackageDescription PackageUnderTest
        => _initializedPackageUnderTest
            ?? throw new InvalidOperationException(
                $"The object of type \"{typeof(PackageDescription)}\" has not been initialized.");

    private PackageManager(
        PackageAnalyzer packageAnalyzer,
        AbsolutePath packageSourceDirectoryPath,
        AbsolutePath globalPackagesDirectoryPath)
    {
        _packageAnalyzer = packageAnalyzer;
        _packageSourceDirectoryPath = packageSourceDirectoryPath;
        GlobalPackagesDirectoryPath = globalPackagesDirectoryPath;
    }

    private async Task<PackageDescription> PushPackageAsync(AbsolutePath filePath)
    {
        await new SystemCommand().RunAsync(
            "dotnet",
            "nuget",
            "push",
            filePath.ToString(),
            "--source",
            _packageSourceDirectoryPath.ToString());

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
            var packageSourceDirectoryPath = workingDirectoryPath / "PackageSource";
            var globalPackagesDirectoryPath = workingDirectoryPath / "GlobalPackages";

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
