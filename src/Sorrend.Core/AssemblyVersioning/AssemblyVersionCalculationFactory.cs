using Sorrend.Core.VersioningSchemes;

namespace Sorrend.Core.AssemblyVersioning;

public class AssemblyVersionCalculationFactory(IVersioningScheme versioningScheme)
{
    public AssemblyVersionCalculation Create()
        => new(versioningScheme);
}
