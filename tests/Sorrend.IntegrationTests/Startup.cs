using Microsoft.Extensions.DependencyInjection;
using Sorrend.IntegrationTests.Tools;
using Sorrend.IntegrationTests.Tools.Assemblies;
using Sorrend.IntegrationTests.Tools.Packages;
using Sorrend.IntegrationTests.Tools.Projects;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.IntegrationTests.Tools.SystemUnderTest;

namespace Sorrend.IntegrationTests;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<TestingEnvironment.Provider>();
        services.AddSingleton<PackageAnalyzer>();
        services.AddSingleton<PackageManager.Provider>();
        services.AddSingleton<ProjectFactory>();
        services.AddSingleton<BuildSystem.Provider>();
        services.AddSingleton<AssemblyAnalyzer>();
        services.AddSingleton<GitRepositoryFactory>();
        services.AddSingleton<SutConfigurationWriter>();
        services.AddSingleton<HeadlessSut>();
    }
}
