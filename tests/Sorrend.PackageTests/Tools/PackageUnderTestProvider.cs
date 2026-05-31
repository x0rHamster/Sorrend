using Sorrend.IntegrationTests.Tools;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Utilities;

namespace Sorrend.PackageTests.Tools;

public class PackageUnderTestProvider(
    TestingEnvironment.Provider testingEnvironmentProvider,
    PackageManager.Provider packageManagerProvider,
    PackageAnalyzer packageAnalyzer)
    : AsyncInitializingProvider<PackageDescription>
{
    protected override async Task<PackageDescription> CreateInitializedAsync()
    {
        var testingEnvironment = await testingEnvironmentProvider.GetAsync();
        var packageManager = await packageManagerProvider.GetAsync();

        var packageFilePath = testingEnvironment.TestAssemblyDirectoryPath / "Sorrend.MsBuildTool.nupkg";
        await packageManager.PublishPackageLocallyAsync(packageFilePath);
        return await packageAnalyzer.LoadAsync(packageFilePath);
    }
}
