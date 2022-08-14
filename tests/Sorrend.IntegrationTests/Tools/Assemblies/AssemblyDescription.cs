using System.Diagnostics;

namespace Sorrend.IntegrationTests.Tools.Assemblies
{
    public class AssemblyDescription
    {
        public string InformationalVersion { get; }

        public AssemblyDescription(FileVersionInfo fileVersionInfo)
        {
            InformationalVersion = fileVersionInfo.ProductVersion;
        }
    }
}
