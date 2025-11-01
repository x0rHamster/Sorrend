using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sorrend.IntegrationTests.Utilities;

namespace Sorrend.IntegrationTests.Tools
{
    public sealed class TestingEnvironment : IDisposable
    {
        private readonly DirectoryInfo _workingDirectory;

        public string WorkingDirectoryPath { get; }

        public string PackageUnderTestFilePath
            => Path.Combine(TestAssemblyDirectoryPath, "Sorrend.MsBuildTool.nupkg");

        private string TestAssemblyDirectoryPath { get; }

        private TestingEnvironment()
        {
            TestAssemblyDirectoryPath = Path.GetDirectoryName(GetType().Assembly.Location)!;

            WorkingDirectoryPath = Path.Combine(
                Path.GetTempPath(),
                $"Sorrend-{DateTime.UtcNow:yyMMdd-HHmm}-{Generate.DirectoryName()}");

            _workingDirectory = new DirectoryInfo(WorkingDirectoryPath);
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
}
