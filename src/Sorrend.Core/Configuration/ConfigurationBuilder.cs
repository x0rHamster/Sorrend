using Sorrend.Core.AssemblyVersioning;
using Sorrend.Core.VersioningSchemes;

namespace Sorrend.Core.Configuration;

public class ConfigurationBuilder
{
    private ConfigurationDocument? _document;

    public void Set(ConfigurationDocument document)
    {
        _document = document;
    }

    public ConfigurationRoot Build()
    {
        var versioningScheme = _document?.VersioningScheme
            ?? VersioningSchemesConfiguration.DefaultVersioningScheme;

        return new ConfigurationRoot(
            new AssemblyVersioningConfiguration(
                new VersioningSchemesConfiguration(versioningScheme)));
    }
}
