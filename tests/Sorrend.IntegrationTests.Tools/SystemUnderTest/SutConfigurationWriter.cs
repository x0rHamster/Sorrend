using System.Xml.Linq;

namespace Sorrend.IntegrationTests.Tools.SystemUnderTest;

public class SutConfigurationWriter
{
    public Task WriteAsync(Action<SutConfigurationSpecification>? configure = null)
    {
        var specification = new SutConfigurationSpecification();
        configure?.Invoke(specification);
        return WriteAsync(specification);
    }

    private Task WriteAsync(SutConfigurationSpecification specification)
    {
        XNamespace ns = "urn:sorrend:v0";

        var document = Configuration(
            Property("VersioningScheme", specification.VersioningScheme));

        document.Save(specification.FilePath.ToString());

        return Task.CompletedTask;

        XDocument Configuration(params object?[] content)
            => new(
                new XDeclaration(null, null, null),
                new XElement(ns + "Configuration", content));

        XElement? Property(string name, object? value)
            => value != null
                ? new XElement(ns + name, value.ToString())
                : null;
    }
}
