using System.IO;
using System.Xml.Serialization;

namespace Sorrend.Core.Configuration;

public class ConfigurationSerializer
{
    private static readonly XmlSerializer XmlSerializer = new(typeof(ConfigurationDocument));

    public ConfigurationDocument Deserialize(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return (ConfigurationDocument)XmlSerializer.Deserialize(stream);
    }
}
