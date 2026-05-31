using System.IO;
using Sorrend.Core.OperatingSystem;
using Sorrend.IntegrationTests.Tools.Utilities;

namespace Sorrend.IntegrationTests.Tools;

public sealed class TestingEnvironment : IDisposable
{
    private readonly DirectoryInfo _workingDirectory;

    public AbsolutePath WorkingDirectoryPath { get; }

    public AbsolutePath TestAssemblyDirectoryPath { get; }

    [SuppressMessage(
        "Major Code Smell",
        "S6354:Use a testable date/time provider",
        Justification = "A developer will search for the working directory using the real time of the test run")]
    private TestingEnvironment()
    {
        TestAssemblyDirectoryPath = AbsolutePath.GetAssemblyDirectory<TestingEnvironment>();

        WorkingDirectoryPath = AbsolutePath.TempDirectory
            / $"Sorrend-{DateTime.UtcNow:yyMMdd-HHmm}-{Generate.DirectoryName()}";

        _workingDirectory = new DirectoryInfo(WorkingDirectoryPath.ToString());
        _workingDirectory.Create();
    }

    public void Dispose()
    {
        UnsetReadOnlyRecursively(_workingDirectory);
        _workingDirectory.Delete(true);
    }

    private static void UnsetReadOnlyRecursively(DirectoryInfo directory)
    {
        var readOnlyItems = directory
            .GetFileSystemInfos("*", SearchOption.AllDirectories)
            .Where(IsReadOnly);

        foreach (var item in readOnlyItems)
        {
            UnsetReadOnly(item);
        }

        bool IsReadOnly(FileSystemInfo item)
            => (item.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

        void UnsetReadOnly(FileSystemInfo item)
            => item.Attributes &= ~FileAttributes.ReadOnly;
    }

    public class Provider : AsyncInitializingProvider<TestingEnvironment>
    {
        protected override Task<TestingEnvironment> CreateInitializedAsync()
            => Task.FromResult(new TestingEnvironment());
    }
}
