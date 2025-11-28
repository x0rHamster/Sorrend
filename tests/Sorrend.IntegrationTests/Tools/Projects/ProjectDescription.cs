using Sorrend.Core.OperatingSystem;

namespace Sorrend.IntegrationTests.Tools.Projects;

public class ProjectDescription
{
    public AbsolutePath DirectoryPath
        => FilePath.ParentDirectory;

    public AbsolutePath FilePath { get; }

    public bool SdkStyle { get; }

    public string BuildConfiguration => "Debug";

    public IReadOnlyCollection<AbsolutePath> AssemblyFilePaths { get; }

    public AbsolutePath AssemblyFilePath
        => AssemblyFilePaths.Count > 1
            ? throw new InvalidOperationException(
                $"The property \"{nameof(AssemblyFilePaths)}\" contains multiple values, use it instead.")
            : AssemblyFilePaths.Single();

    public AbsolutePath PackageFilePath
        => DirectoryPath
            / "bin"
            / BuildConfiguration
            / $"{FilePath.BaseNameWithoutExtensions}.nupkg";

    public ProjectDescription(AbsolutePath filePath, ProjectSpecification specification)
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
            .Select(x => x / $"{FilePath.BaseNameWithoutExtensions}.dll")
            .ToArray();
    }

    private AbsolutePath GetOutputDirectoryPath(string directoryName, string targetFrameworkMoniker)
    {
        var result = DirectoryPath / directoryName / BuildConfiguration;

        if (SdkStyle)
        {
            result /= targetFrameworkMoniker;
        }

        return result;
    }
}
