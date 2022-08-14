using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace Sorrend.IntegrationTests.Tools.Projects
{
    public static partial class ProjectTranslator
    {
        private static readonly XNamespace ProjectXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public static XDocument GetProjectXml(
            ProjectSpecification specification,
            string globalPackagesDirectoryPath)
        {
            return specification.EffectiveSdkStyle
                ? GetSdkStyleProjectXml(specification)
                : GetNonSdkStyleProjectXml(
                    specification,
                    globalPackagesDirectoryPath);
        }

        private static XDocument GetSdkStyleProjectXml(ProjectSpecification specification)
        {
            var targetFrameworkProperty = specification.TargetFrameworks.Count > 1
                ? Property(
                    "TargetFrameworks",
                    string.Join(";", specification.TargetFrameworks.Select(GetTargetFrameworkMoniker)))
                : Property(
                    "TargetFramework",
                    GetTargetFrameworkMoniker(specification.EffectiveTargetFramework));

            return Project(
                PropertyGroup(
                    targetFrameworkProperty),
                ItemGroup(
                    specification.PackageReferences
                        .Select(x => PackageReference(x.Id, x.Version, x.DevelopmentDependency))));

            XDocument Project(params object[] content)
                => new XDocument(
                    new XElement(
                        "Project",
                        new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                        content));

            XElement PropertyGroup(params object[] properties)
                => new XElement("PropertyGroup", properties);

            XElement Property(string name, string value)
                => new XElement(name, value);

            XElement ItemGroup(params object[] items)
                => new XElement("ItemGroup", items);

            XElement PackageReference(string id, string version, bool developmentDependency)
                => new XElement(
                    "PackageReference",
                    new XAttribute("Include", id),
                    new XAttribute("Version", version),
                    developmentDependency
                        ? new XAttribute("PrivateAssets", "all")
                        : null);
        }

        [SuppressMessage(
            "Major Code Smell",
            "S1854:Unused assignments should be removed",
            Justification = "False positive")]
        private static XDocument GetNonSdkStyleProjectXml(
            ProjectSpecification specification,
            string globalPackagesDirectoryPath)
        {
            var ns = ProjectXmlNamespace;

            return Project(
                Import(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"),
                specification.PackageReferences
                    .Where(x => x.BuildProps.Exists)
                    .Select(x => x.BuildProps.GetLegacyFilePath(globalPackagesDirectoryPath))
                    .Select(Import),
                PropertyGroup(
                    Property("OutputType", "Library"),
                    Property(
                        "TargetFrameworkVersion",
                        GetTargetFrameworkVersion(specification.EffectiveTargetFramework))),
                Import(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets"),
                specification.PackageReferences
                    .Where(x => x.BuildTargets.Exists)
                    .Select(x => x.BuildTargets.GetLegacyFilePath(globalPackagesDirectoryPath))
                    .Select(Import));

            XDocument Project(params object[] content)
                => new XDocument(
                    new XDeclaration(null, null, null),
                    new XElement(ns + "Project", content));

            XElement Import(string projectFilePath)
                => new XElement(ns + "Import", new XAttribute("Project", projectFilePath));

            XElement PropertyGroup(params object[] properties)
                => new XElement(ns + "PropertyGroup", properties);

            XElement Property(string name, string value)
                => new XElement(ns + name, value);
        }

        public static XDocument GetPackagesConfigXml(ProjectSpecification specification)
        {
            return Packages(
                specification.PackageReferences
                    .Select(x => Package(x.Id, x.Version, x.DevelopmentDependency)));

            XDocument Packages(params object[] content)
                => new XDocument(
                    new XDeclaration(null, null, null),
                    new XElement("packages", content));

            XElement Package(string id, string version, bool developmentDependency)
                => new XElement(
                    "package",
                    new XAttribute("id", id),
                    new XAttribute("version", version),
                    new XAttribute(
                        "targetFramework",
                        GetTargetFrameworkMoniker(specification.EffectiveTargetFramework)),
                    developmentDependency
                        ? new XAttribute("developmentDependency", true)
                        : null);
        }
    }
}
