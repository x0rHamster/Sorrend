using System.IO.Compression;
using System.Xml.Linq;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Packages;

public class PackageAnalyzer
{
    public Task<PackageDescription> LoadAsync(AbsolutePath filePath)
    {
        using var archive = ZipFile.OpenRead(filePath.ToString());

        var archiveEntryPaths = archive.Entries
            .Select(x => new RelativePath(x.FullName))
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
