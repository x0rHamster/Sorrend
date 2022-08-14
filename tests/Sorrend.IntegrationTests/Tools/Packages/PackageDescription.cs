using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Sorrend.IntegrationTests.Utilities;
using Xunit;

namespace Sorrend.IntegrationTests.Tools.Packages
{
    public class PackageDescription
    {
        private static readonly XNamespace[] NuspecXmlNamespaces =
        {
            "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd",
            "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd",
            "http://schemas.microsoft.com/packaging/2011/10/nuspec.xsd",
            "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd",
            "http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd",
            "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd",
        };

        public string Id { get; }

        public string Version { get; }

        public bool DevelopmentDependency { get; }

        public IReadOnlyCollection<string> DependencyPackageIds { get; }

        public File BuildProps { get; }

        public File BuildTargets { get; }

        public PackageDescription(
            XDocument nuspecXml,
            IReadOnlyCollection<string> packageFilePathsRelativeToPackage)
        {
            var ns = nuspecXml.Root?.GetDefaultNamespace();
            Assert.NotNull(ns);
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
                ?? Array.Empty<string>();

            BuildProps = FindFile(packageFilePathsRelativeToPackage, $"build/{Id}.props");
            BuildTargets = FindFile(packageFilePathsRelativeToPackage, $"build/{Id}.targets");
        }

        public static bool IsNuspec(string filePathRelativeToPackage)
            => Regex.IsMatch(filePathRelativeToPackage, @"^[^/]+\.nuspec$", RegexOptions.IgnoreCase);

        private File FindFile(
            IEnumerable<string> packageFilePathsRelativeToPackage,
            string filePathRelativeToPackage)
        {
            return new File(
                this,
                filePathRelativeToPackage,
                packageFilePathsRelativeToPackage.Contains(filePathRelativeToPackage));
        }

        public class File
        {
            private readonly PackageDescription _package;
            private readonly string _filePathRelativeToPackage;

            public bool Exists { get; }

            public File(
                PackageDescription package,
                string filePathRelativeToPackage,
                bool exists)
            {
                Exists = exists;
                _package = package;
                _filePathRelativeToPackage = filePathRelativeToPackage;
            }

            public string GetLegacyFilePath(string globalPackagesDirectoryPath)
            {
                if (!Exists)
                {
                    throw new InvalidOperationException(
                        $"The package \"{_package.Id}\" does not contain a file \"{_filePathRelativeToPackage}\".");
                }

                return Path.Combine(
                    globalPackagesDirectoryPath,
                    $"{_package.Id}.{_package.Version}",
                    _filePathRelativeToPackage);
            }
        }
    }
}
