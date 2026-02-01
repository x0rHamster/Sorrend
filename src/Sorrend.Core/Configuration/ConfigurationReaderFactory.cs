namespace Sorrend.Core.Configuration;

public class ConfigurationReaderFactory(ConfigurationSerializer configurationSerializer)
{
    public ConfigurationReader Create()
        => new(configurationSerializer);
}
