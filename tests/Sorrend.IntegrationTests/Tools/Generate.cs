namespace Sorrend.IntegrationTests.Tools;

public static class Generate
{
    public static string DirectoryName()
        => Path.GetRandomFileName().Remove(8, 1);

    public static string FileName()
        => Path.GetRandomFileName();
}
