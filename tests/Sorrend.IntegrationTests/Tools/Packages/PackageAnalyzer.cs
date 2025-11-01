using System;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sorrend.IntegrationTests.Tools.Packages
{
    public class PackageAnalyzer
    {
        public Task<PackageDescription> LoadAsync(string filePath)
        {
            using var archive = ZipFile.OpenRead(filePath);

            var archiveEntryPaths = archive.Entries
                .Select(x => x.FullName)
                .ToArray();

            var nuspecArchiveEntryIndex = Array.FindIndex(
                archiveEntryPaths,
                PackageDescription.IsNuspec);

            using var nuspecStream = archive.Entries[nuspecArchiveEntryIndex].Open();

            var package = new PackageDescription(
                XDocument.Load(nuspecStream),
                archiveEntryPaths);

            return Task.FromResult(package);
        }
    }
}
