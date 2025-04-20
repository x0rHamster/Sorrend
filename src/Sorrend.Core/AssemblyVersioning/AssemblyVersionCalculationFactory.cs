namespace Sorrend.Core.AssemblyVersioning
{
    public class AssemblyVersionCalculationFactory
    {
        private readonly SemanticVersioningScheme _versioningScheme;

        public AssemblyVersionCalculationFactory(SemanticVersioningScheme versioningScheme)
        {
            _versioningScheme = versioningScheme;
        }

        public AssemblyVersionCalculation Create()
            => new AssemblyVersionCalculation(_versioningScheme);
    }
}
