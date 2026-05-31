using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Assemblies;

public class AssemblyAnalyzer
{
    public Task<AssemblyDescription> LoadAsync(AbsolutePath filePath)
    {
        var resolver = new PathAssemblyResolver(
            Directory.GetFiles(
                RuntimeEnvironment.GetRuntimeDirectory(),
                "*.dll"));

        using var context = new MetadataLoadContext(resolver);

        var assembly = context.LoadFromAssemblyPath(filePath.ToString());
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath.ToString());

        return Task.FromResult(new AssemblyDescription(assembly, fileVersionInfo));
    }
}
