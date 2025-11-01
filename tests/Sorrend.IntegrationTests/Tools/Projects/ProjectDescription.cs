namespace Sorrend.IntegrationTests.Tools.Projects;

public class ProjectDescription
{
    public string DirectoryPath
        => Path.GetDirectoryName(FilePath)!;

    public string FilePath { get; }

    public bool SdkStyle { get; }

    public string BuildConfiguration => "Debug";

    public IReadOnlyCollection<string> AssemblyFilePaths { get; }

    public string AssemblyFilePath
        => AssemblyFilePaths.Count > 1
            ? throw new InvalidOperationException(
                $"The property \"{nameof(AssemblyFilePaths)}\" contains multiple values, use it instead.")
            : AssemblyFilePaths.Single();

    public string PackageFilePath
        => Path.Combine(
            DirectoryPath,
            "bin",
            BuildConfiguration,
            $"{Path.GetFileNameWithoutExtension(FilePath)}.nupkg");

    public ProjectDescription(string filePath, ProjectSpecification specification)
    {
        var targetFrameworks = specification.TargetFrameworks.Any()
            ? specification.TargetFrameworks.AsEnumerable()
            : [specification.EffectiveTargetFramework];

        var targetFrameworkMonikers = targetFrameworks
            .Select(ProjectTranslator.GetTargetFrameworkMoniker)
            .ToArray();

        FilePath = filePath;
        SdkStyle = specification.EffectiveSdkStyle;

        AssemblyFilePaths = targetFrameworkMonikers
            .Select(tfm => GetOutputDirectoryPath("bin", tfm))
            .Select(x => Path.Combine(x, $"{Path.GetFileNameWithoutExtension(FilePath)}.dll"))
            .ToArray();
    }

    private string GetOutputDirectoryPath(string directoryName, string targetFrameworkMoniker)
    {
        var parts = new List<string> { DirectoryPath, directoryName, BuildConfiguration };

        if (SdkStyle)
        {
            parts.Add(targetFrameworkMoniker);
        }

        return Path.Combine(parts.ToArray());
    }
}
