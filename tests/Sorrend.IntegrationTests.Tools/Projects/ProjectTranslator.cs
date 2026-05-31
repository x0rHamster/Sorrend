using System.Xml.Linq;
using Sorrend.Core.OperatingSystem;
using Sorrend.IntegrationTests.Tools.Utilities;
using Xunit;

namespace Sorrend.IntegrationTests.Tools.Projects;

[SuppressMessage(
    "Minor Code Smell",
    "S1192:String literals should not be duplicated",
    Justification = "A match of the XML document identifiers is a false duplication")]
public static partial class ProjectTranslator
{
    private const string ChangeTokenPropertyName = "SorrendTestProjectChangeToken";

    private static readonly XNamespace ProjectXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

    public static XDocument GetProjectXml(
        ProjectSpecification specification,
        AbsolutePath globalPackagesDirectoryPath)
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
                targetFrameworkProperty,
                Property(
                    ChangeTokenPropertyName,
                    specification.ChangeToken.ToString())),
            ItemGroup(
                specification.PackageReferences
                    .Select(x => PackageReference(x.Id, x.Version, x.DevelopmentDependency))));

        XDocument Project(params object[] content)
            => new(
                new XElement(
                    "Project",
                    new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                    content));

        XElement PropertyGroup(params object[] properties)
            => new("PropertyGroup", properties);

        XElement Property(string name, string value)
            => new(name, value);

        XElement ItemGroup(params object[] items)
            => new("ItemGroup", items);

        XElement PackageReference(string id, string version, bool developmentDependency)
            => new(
                "PackageReference",
                new XAttribute("Include", id),
                new XAttribute("Version", version),
                developmentDependency
                    ? new XAttribute("PrivateAssets", "all")
                    : null);
    }

    private static XDocument GetNonSdkStyleProjectXml(
        ProjectSpecification specification,
        AbsolutePath globalPackagesDirectoryPath)
    {
        var ns = ProjectXmlNamespace;

        return Project(
            Import(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"),
            specification.PackageReferences
                .Where(x => x.BuildProps.Exists)
                .Select(x => x.BuildProps.GetLegacyFilePath(globalPackagesDirectoryPath))
                .Select(x => Import(x.ToString())),
            PropertyGroup(
                Property("OutputType", "Library"),
                Property(
                    "TargetFrameworkVersion",
                    GetTargetFrameworkVersion(specification.EffectiveTargetFramework)),
                Property(
                    ChangeTokenPropertyName,
                    specification.ChangeToken.ToString())),
            Import(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets"),
            specification.PackageReferences
                .Where(x => x.BuildTargets.Exists)
                .Select(x => x.BuildTargets.GetLegacyFilePath(globalPackagesDirectoryPath))
                .Select(x => Import(x.ToString())));

        XDocument Project(params object[] content)
            => new(
                new XDeclaration(null, null, null),
                new XElement(ns + "Project", content));

        XElement Import(string projectFilePath)
            => new(ns + "Import", new XAttribute("Project", projectFilePath));

        XElement PropertyGroup(params object[] properties)
            => new(ns + "PropertyGroup", properties);

        XElement Property(string name, string value)
            => new(ns + name, value);
    }

    public static void SetProjectChangeToken(XDocument projectXml, Guid changeToken)
    {
        var ns = projectXml.Root?.GetDefaultNamespace();
        if (!string.IsNullOrEmpty(ns?.NamespaceName))
        {
            Assert.Equal(ProjectXmlNamespace, ns);
        }

        var changeTokenElement = projectXml
            .ElementOrThrow(ns + "Project")
            .Elements(ns + "PropertyGroup")
            .SelectMany(x => x.Elements(ns + ChangeTokenPropertyName))
            .Single();

        changeTokenElement.Value = changeToken.ToString();
    }

    public static XDocument GetPackagesConfigXml(ProjectSpecification specification)
    {
        return Packages(
            specification.PackageReferences
                .Select(x => Package(x.Id, x.Version, x.DevelopmentDependency)));

        XDocument Packages(params object[] content)
            => new(
                new XDeclaration(null, null, null),
                new XElement("packages", content));

        XElement Package(string id, string version, bool developmentDependency)
            => new(
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
