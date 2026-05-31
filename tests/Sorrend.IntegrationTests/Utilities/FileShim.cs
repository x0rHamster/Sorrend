using System.IO;
using System.Threading;

namespace Sorrend.IntegrationTests.Utilities;

public static class FileShim
{
    public static Task WriteAllTextAsync(string path, string? contents, CancellationToken ct)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        return File.WriteAllTextAsync(path, contents, ct);
#else
        ct.ThrowIfCancellationRequested();
        File.WriteAllText(path, contents);
        return Task.CompletedTask;
#endif
    }
}
