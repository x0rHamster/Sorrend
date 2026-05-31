using System.IO;

namespace Sorrend.IntegrationTests.Tools;

public static class Generate
{
    public static string DirectoryName()
        => Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

    public static string FileName()
        => Path.GetRandomFileName();
}
