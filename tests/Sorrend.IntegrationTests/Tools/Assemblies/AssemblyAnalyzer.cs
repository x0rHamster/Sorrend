using System.Diagnostics;
using System.Threading.Tasks;

namespace Sorrend.IntegrationTests.Tools.Assemblies
{
    public class AssemblyAnalyzer
    {
        public Task<AssemblyDescription> LoadAsync(string filePath)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return Task.FromResult(new AssemblyDescription(fileVersionInfo));
        }
    }
}
