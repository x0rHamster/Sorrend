using System.Xml.Linq;

namespace Sorrend.IntegrationTests.Tools.Packages
{
    public static class PackageTranslator
    {
        public static XDocument GetNugetConfigXml(
            string packageSourceDirectoryPath,
            string globalPackagesDirectoryPath)
        {
            return Configuration(
                Section(
                    "config",
                    Item("globalPackagesFolder", globalPackagesDirectoryPath),
                    Item("repositoryPath", globalPackagesDirectoryPath)),
                Section(
                    "packageSources",
                    Item("local", packageSourceDirectoryPath)));

            XDocument Configuration(params object[] sections)
                => new XDocument(
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
                => new XElement(
                    "add",
                    new XAttribute("key", key),
                    new XAttribute("value", value));
        }
    }
}
