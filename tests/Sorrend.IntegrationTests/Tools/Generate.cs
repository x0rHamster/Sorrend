using System.IO;

namespace Sorrend.IntegrationTests.Tools
{
    public static class Generate
    {
        public static string DirectoryName()
            => Path.GetRandomFileName().Remove(8, 1);
    }
}
