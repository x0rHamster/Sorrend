using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sorrend.Core;
using Sorrend.Core.VersionControl;

namespace Sorrend.IntegrationTests.Tools.SystemUnderTest;

public sealed class HeadlessSut : IDisposable
{
    private readonly TestingEnvironment.Provider _testingEnvironmentProvider;
    private readonly ServiceProvider _provider;

    public GitVersionControlSystem GitVersionControlSystem
        => _provider.GetRequiredService<GitVersionControlSystem>();

    public HeadlessSut(TestingEnvironment.Provider testingEnvironmentProvider)
    {
        _testingEnvironmentProvider = testingEnvironmentProvider;

        var services = new ServiceCollection();
        services.AddSorrendCore();
        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    public async Task<AssemblyVersionFileContent> CalculateAssemblyVersionAsync(string projectFilePath)
    {
        var testingEnvironment = await _testingEnvironmentProvider.GetAsync();

        var workingDirectoryPath = testingEnvironment.WorkingDirectoryPath;
        var assemblyVersionFilePath = Path.Combine(workingDirectoryPath, Generate.FileName());

        await _provider
            .GetRequiredService<ApplicationServices>()
            .CalculateAssemblyVersionAsync(
                testingEnvironment.WorkingDirectoryPath,
                projectFilePath,
                assemblyVersionFilePath);

        var assemblyVersionXml = XDocument.Load(assemblyVersionFilePath);
        return new AssemblyVersionFileContent(assemblyVersionXml);
    }
}
