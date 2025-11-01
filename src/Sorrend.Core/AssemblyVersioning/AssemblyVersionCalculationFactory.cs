namespace Sorrend.Core.AssemblyVersioning;

public class AssemblyVersionCalculationFactory(SemanticVersioningScheme versioningScheme)
{
    public AssemblyVersionCalculation Create()
        => new(versioningScheme);
}
