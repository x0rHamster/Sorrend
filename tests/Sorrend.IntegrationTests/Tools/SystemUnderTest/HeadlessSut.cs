using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sorrend.Core;
using Sorrend.Core.OperatingSystem;
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

    public async Task<AssemblyVersionFileContent> CalculateAssemblyVersionAsync(AbsolutePath projectFilePath)
    {
        var testingEnvironment = await _testingEnvironmentProvider.GetAsync();

        var workingDirectoryPath = testingEnvironment.WorkingDirectoryPath;
        var assemblyVersionFilePath = workingDirectoryPath / Generate.FileName();

        await _provider
            .GetRequiredService<ApplicationServices>()
            .CalculateAssemblyVersionAsync(projectFilePath, assemblyVersionFilePath);

        var assemblyVersionXml = XDocument.Load(assemblyVersionFilePath.ToString());
        return new AssemblyVersionFileContent(assemblyVersionXml);
    }
}
