using System.Xml.Linq;

namespace Sorrend.IntegrationTests.Utilities;

public static class XmlExtensions
{
    public static XElement ElementOrThrow(this XContainer container, XName name)
    {
        return container.Element(name)
            ?? throw new InvalidOperationException(
                container is XElement element
                    ? $"The XML element \"{element.Name.LocalName}\" does not contain an element \"{name}\"."
                    : $"The XML element \"{name}\" was not found.");
    }

    public static XAttribute AttributeOrThrow(this XElement element, XName name)
    {
        return element.Attribute(name)
            ?? throw new InvalidOperationException(
                $"The XML element \"{element.Name.LocalName}\" does not contain an attribute \"{name}\".");
    }
}
