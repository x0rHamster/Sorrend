using System.Diagnostics;
using System.Reflection;

namespace Sorrend.IntegrationTests.Tools.Assemblies;

public class AssemblyDescription
{
    public string AssemblyVersion { get; }

    public string FileVersion { get; }

    public string InformationalVersion { get; }

    public AssemblyDescription(Assembly assembly, FileVersionInfo fileVersionInfo)
    {
        AssemblyVersion = assembly.GetName().Version.ToString();
        FileVersion = fileVersionInfo.FileVersion;
        InformationalVersion = fileVersionInfo.ProductVersion;
    }
}
