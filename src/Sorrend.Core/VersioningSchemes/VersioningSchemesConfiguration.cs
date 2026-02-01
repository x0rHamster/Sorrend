namespace Sorrend.Core.VersioningSchemes;

public class VersioningSchemesConfiguration(VersioningSchemeIdentifier? versioningScheme)
{
    public const VersioningSchemeIdentifier DefaultVersioningScheme
        = VersioningSchemeIdentifier.SemanticVersioning;

    public VersioningSchemeIdentifier VersioningScheme { get; }
        = versioningScheme ?? DefaultVersioningScheme;
}
