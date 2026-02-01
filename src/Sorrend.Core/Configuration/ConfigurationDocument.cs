using System.Xml;
using System.Xml.Serialization;
using Sorrend.Core.VersioningSchemes;

namespace Sorrend.Core.Configuration;

[XmlRoot("Configuration", Namespace = XmlNamespace)]
public class ConfigurationDocument
{
    private const string XmlNamespace = "urn:sorrend:v0";

    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces XmlNamespaces { get; set; }
        = new([new XmlQualifiedName(string.Empty, XmlNamespace)]);

    public VersioningSchemeIdentifier? VersioningScheme { get; set; }
}
