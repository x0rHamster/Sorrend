using System.Xml.Linq;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Packages;

public static class PackageTranslator
{
    public static XDocument GetNugetConfigXml(
        AbsolutePath packageSourceDirectoryPath,
        AbsolutePath globalPackagesDirectoryPath)
    {
        return Configuration(
            Section(
                "config",
                Item("globalPackagesFolder", globalPackagesDirectoryPath.ToString()),
                Item("repositoryPath", globalPackagesDirectoryPath.ToString())),
            Section(
                "packageSources",
                Item("local", packageSourceDirectoryPath.ToString())));

        XDocument Configuration(params object[] sections)
            => new(
                new XDeclaration(null, null, null),
                new XElement("configuration", sections));

        XElement Section(string name, params object[] items)
        {
            var element = new XElement(name);
            element.Add(new XElement("clear"));
            element.Add(items);
            return element;
        }

        XElement Item(string key, string value)
            => new(
                "add",
                new XAttribute("key", key),
                new XAttribute("value", value));
    }
}
