using System.Linq;
using System.Xml.Linq;
using Sorrend.IntegrationTests.Utilities;

namespace Sorrend.IntegrationTests.Tools.SystemUnderTest
{
    public class AssemblyVersionFileContent
    {
        public string AssemblyVersion { get; }

        public string FileVersion { get; }

        public string InformationalVersion { get; }

        public string PackageVersion { get; }

        public AssemblyVersionFileContent(XDocument assemblyVersionXml)
        {
            XNamespace ns = "urn:sorrend:v0";

            var rootElement = assemblyVersionXml.ElementOrThrow(ns + "AssemblyVersion");

            var properties = rootElement
                .ElementOrThrow(ns + "ProjectProperties")
                .Elements(ns + "ProjectProperty")
                .ToDictionary(
                    x => x.AttributeOrThrow("Name").Value,
                    x => x.AttributeOrThrow("Value").Value);

            PackageVersion = properties["PackageVersion"];

            var attributes = rootElement
                .ElementOrThrow(ns + "AssemblyAttributes")
                .Elements(ns + "AssemblyAttribute")
                .ToDictionary(
                    x => x.AttributeOrThrow("TypeName").Value,
                    x => x.AttributeOrThrow("Parameter1").Value);

            AssemblyVersion = attributes["System.Reflection.AssemblyVersionAttribute"];
            FileVersion = attributes["System.Reflection.AssemblyFileVersionAttribute"];
            InformationalVersion = attributes["System.Reflection.AssemblyInformationalVersionAttribute"];
        }
    }
}
