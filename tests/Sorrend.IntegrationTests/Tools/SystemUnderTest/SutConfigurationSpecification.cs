using Sorrend.Core.OperatingSystem;
using Sorrend.Core.VersioningSchemes;

namespace Sorrend.IntegrationTests.Tools.SystemUnderTest;

public class SutConfigurationSpecification
{
    private const string FileName = "sorrend.config.xml";

    private AbsolutePath? _filePath;

    public AbsolutePath FilePath
    {
        get => _filePath ?? throw new InvalidOperationException("The configuration file path cannot be empty.");
        set => _filePath = value;
    }

    public VersioningSchemeIdentifier? VersioningScheme { get; set; }

    public SutConfigurationSpecification WithFileDirectory(AbsolutePath directoryPath)
    {
        FilePath = directoryPath / FileName;
        return this;
    }

    public SutConfigurationSpecification WithVersioningScheme(VersioningSchemeIdentifier versioningScheme)
    {
        VersioningScheme = versioningScheme;
        return this;
    }
}
