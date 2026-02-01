using System.IO;
using Sorrend.Core.OperatingSystem;

namespace Sorrend.Core.Configuration;

public class ConfigurationReader(ConfigurationSerializer configurationSerializer)
{
    private const string FileName = "sorrend.config.xml";

    private readonly ConfigurationBuilder _builder = new();

    public Task ReadWorkingCopyAsync(AbsolutePath projectDirectoryPath)
    {
        var directoryPath = projectDirectoryPath;

        while (true)
        {
            var filePath = directoryPath / FileName;

            if (File.Exists(filePath.ToString()))
            {
                var documentBytes = File.ReadAllBytes(filePath.ToString());
                var document = configurationSerializer.Deserialize(documentBytes);
                _builder.Set(document);

                break;
            }

            if (directoryPath.Components.Count == 0)
            {
                break;
            }

            directoryPath = directoryPath.ParentDirectory;
        }

        return Task.CompletedTask;
    }

    public ConfigurationRoot GetCurrentConfiguration()
        => _builder.Build();
}
