using Sorrend.Core.VersioningSchemes;

namespace Sorrend.Core.AssemblyVersioning;

public class AssemblyVersioningConfiguration(VersioningSchemesConfiguration versioningSchemes)
{
    public VersioningSchemesConfiguration VersioningSchemes { get; }
        = versioningSchemes;
}
