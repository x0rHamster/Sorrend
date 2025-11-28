using System.Xml.Linq;
using Sorrend.Core.OperatingSystem;
using Sorrend.IntegrationTests.Utilities;

namespace Sorrend.IntegrationTests.Tools.Packages;

public class PackageDescription
{
    private static readonly XNamespace[] NuspecXmlNamespaces =
    [
        "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd",
        "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd",
        "http://schemas.microsoft.com/packaging/2011/10/nuspec.xsd",
        "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd",
        "http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd",
        "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd",
    ];

    public string Id { get; }

    public string Version { get; }

    public bool DevelopmentDependency { get; }

    public IReadOnlyCollection<string> DependencyPackageIds { get; }

    public File BuildProps { get; }

    public File BuildTargets { get; }

    public PackageDescription(
        XDocument nuspecXml,
        IReadOnlyCollection<RelativePath> packageFilePathsRelativeToPackage)
    {
        var ns = nuspecXml.Root?.GetDefaultNamespace();
        Assert.Contains(ns, NuspecXmlNamespaces);

        var nuspecPackageMetadata = nuspecXml
            .ElementOrThrow(ns + "package")
            .ElementOrThrow(ns + "metadata");

        Id = nuspecPackageMetadata.ElementOrThrow(ns + "id").Value;
        Version = nuspecPackageMetadata.ElementOrThrow(ns + "version").Value;

        DevelopmentDependency = bool.TryParse(
                nuspecPackageMetadata.Element(ns + "developmentDependency")?.Value,
                out var developmentDependency)
            && developmentDependency;

        DependencyPackageIds = nuspecPackageMetadata
                .Element(ns + "dependencies")
                ?.Descendants(ns + "dependency")
                .Select(x => x.AttributeOrThrow("id").Value)
                .Distinct()
                .ToArray()
            ?? [];

        BuildProps = FindFile(
            packageFilePathsRelativeToPackage,
            new RelativePath($"build/{Id}.props"));

        BuildTargets = FindFile(
            packageFilePathsRelativeToPackage,
            new RelativePath($"build/{Id}.targets"));
    }

    public static bool IsNuspec(RelativePath filePathRelativeToPackage)
        => filePathRelativeToPackage.Components.Count == 1
            && !filePathRelativeToPackage.HasRelativeComponents
            && filePathRelativeToPackage.EndsWithExtension("nuspec");

    private File FindFile(
        IEnumerable<RelativePath> packageFilePathsRelativeToPackage,
        RelativePath filePathRelativeToPackage)
    {
        return new File(
            this,
            filePathRelativeToPackage,
            packageFilePathsRelativeToPackage.Contains(filePathRelativeToPackage));
    }

    public class File(
        PackageDescription package,
        RelativePath filePathRelativeToPackage,
        bool exists)
    {
        public bool Exists => exists;

        public AbsolutePath GetLegacyFilePath(AbsolutePath globalPackagesDirectoryPath)
        {
            if (!exists)
            {
                throw new InvalidOperationException(
                    $"The package \"{package.Id}\" does not contain a file \"{filePathRelativeToPackage}\".");
            }

            return globalPackagesDirectoryPath
                / $"{package.Id}.{package.Version}"
                / filePathRelativeToPackage;
        }
    }
}
