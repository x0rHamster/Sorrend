using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sorrend.IntegrationTests.Tools.Assemblies;

public class AssemblyAnalyzer
{
    public Task<AssemblyDescription> LoadAsync(string filePath)
    {
        var resolver = new PathAssemblyResolver(
            Directory.GetFiles(
                RuntimeEnvironment.GetRuntimeDirectory(),
                "*.dll"));

        using var context = new MetadataLoadContext(resolver);

        var assembly = context.LoadFromAssemblyPath(filePath);
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);

        return Task.FromResult(new AssemblyDescription(assembly, fileVersionInfo));
    }
}
