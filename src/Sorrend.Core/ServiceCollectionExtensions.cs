using Microsoft.Extensions.DependencyInjection;
using Sorrend.Core.AssemblyVersioning;
using Sorrend.Core.Configuration;
using Sorrend.Core.VersionControl;
using Sorrend.Core.VersioningSchemes;

namespace Sorrend.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSorrendCore(this IServiceCollection services)
    {
        services.AddSingleton<ConfigurationSerializer>();
        services.AddSingleton<ConfigurationReaderFactory>();

        services.AddSingleton<GitVersionControlSystem>();
        services.AddSingleton<RepositoryLocator>();

        services.AddSingleton<CommitVersionParser>();
        services.AddSingleton<SemanticVersioningScheme>();
        services.AddSingleton<CalendarVersioningScheme>();
        services.AddSingleton<IVersioningScheme, VersioningSchemeDispatcher>();

        services.AddSingleton<AssemblyVersionCalculationFactory>();
        services.AddSingleton<AssemblyVersionSerializer>();

        services.AddSingleton<ApplicationServices>();

        return services;
    }
}
