using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sorrend.Core.Utilities;

namespace Sorrend.Core.AssemblyVersioning;

public class AssemblyVersionSerializer
{
    private static readonly XmlWriterSettings XmlWriterSettings = new()
    {
        Encoding = new UTF8Encoding(),
        Indent = true,
    };

    public byte[] Serialize(AssemblyVersionProperties properties)
    {
        XNamespace ns = "urn:sorrend:v0";

        var document = AssemblyVersion(
            ProjectProperties(
                ProjectProperty("Version", properties.Version),
                ProjectProperty("VersionPrefix", properties.VersionPrefix),
                ProjectProperty("VersionSuffix", properties.VersionSuffix),
                ProjectProperty("AssemblyVersion", properties.AssemblyVersion),
                ProjectProperty("FileVersion", properties.FileVersion),
                ProjectProperty("InformationalVersion", properties.InformationalVersion),
                ProjectProperty("PackageVersion", properties.PackageVersion),
                ProjectProperty("GenerateAssemblyVersionAttribute", false),
                ProjectProperty("GenerateAssemblyFileVersionAttribute", false),
                ProjectProperty("GenerateAssemblyInformationalVersionAttribute", false),
                ProjectProperty("IncludeSourceRevisionInInformationalVersion", false)),
            AssemblyAttributes(
                AssemblyAttribute<AssemblyVersionAttribute>(properties.AssemblyVersion),
                AssemblyAttribute<AssemblyFileVersionAttribute>(properties.FileVersion),
                AssemblyAttribute<AssemblyInformationalVersionAttribute>(properties.InformationalVersion)));

        return Serialize(document);

        XDocument AssemblyVersion(params object[] content)
            => new(
                new XDeclaration(null, null, null),
                new XElement(ns + "AssemblyVersion", content));

        XElement ProjectProperties(params object[] content)
            => new(ns + "ProjectProperties", content);

        XElement ProjectProperty(string name, object value)
            => new(
                ns + "ProjectProperty",
                new XAttribute("Name", name),
                new XAttribute("Value", value.ToString()));

        XElement AssemblyAttributes(params object[] content)
            => new(ns + "AssemblyAttributes", content);

        XElement AssemblyAttribute<T>(string parameter1)
            where T : Attribute
            => new(
                ns + "AssemblyAttribute",
                new XAttribute("TypeName", ReflectionHelper.GetTypeName<T>()),
                new XAttribute("Parameter1", parameter1));
    }

    private static byte[] Serialize(XDocument document)
    {
        using var stream = new MemoryStream();

        using (var writer = XmlWriter.Create(stream, XmlWriterSettings))
        {
            document.WriteTo(writer);
        }

        return stream.ToArray();
    }
}
