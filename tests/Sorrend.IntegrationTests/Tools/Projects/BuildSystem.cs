using System.IO;
using System.Text.RegularExpressions;
using Sorrend.Core.OperatingSystem;
using Sorrend.IntegrationTests.Utilities;

namespace Sorrend.IntegrationTests.Tools.Projects;

public class BuildSystem
{
    private readonly AbsolutePath? _msbuildExecutablePath;

    private BuildSystem(AbsolutePath? msbuildExecutablePath)
    {
        _msbuildExecutablePath = msbuildExecutablePath;
    }

    public Task CleanAsync(ProjectDescription project)
    {
        return project.SdkStyle
            ? RunDotnetCleanAsync(project)
            : RunMsBuildCleanAsync(project);
    }

    private async Task RunDotnetCleanAsync(ProjectDescription project)
        => await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync("dotnet", "clean");

    private async Task RunMsBuildCleanAsync(ProjectDescription project)
        => await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync(
                _msbuildExecutablePath?.ToString() ?? "msbuild",
                "-target:Clean");

    public Task BuildAsync(ProjectDescription project)
        => project.SdkStyle
            ? BuildUsingDotnetBuildAsync(project)
            : BuildUsingMsBuildAsync(project);

    public async Task BuildUsingDotnetBuildAsync(ProjectDescription project)
    {
        // run an explicit restore to exclude its output from the build step
        await RunDotnetRestoreAsync(project);
        await RunDotnetBuildAsync(project);
    }

    public async Task BuildUsingMsBuildAsync(ProjectDescription project)
    {
        await (project.SdkStyle
            ? RunDotnetRestoreAsync(project)
            : RunNugetRestoreAsync(project));

        await RunMsBuildAsync(project);
    }

    private async Task RunDotnetRestoreAsync(ProjectDescription project)
        => await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync("dotnet", "restore");

    private async Task RunDotnetBuildAsync(ProjectDescription project)
        => await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync(
                "dotnet",
                "build",
                "--nologo",
                "--no-restore",
                "--configuration",
                project.BuildConfiguration);

    private async Task RunMsBuildAsync(ProjectDescription project)
        => await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync(
                _msbuildExecutablePath?.ToString() ?? "msbuild",
                "-nologo",
                "-verbosity:minimal",
                $"-property:Configuration={project.BuildConfiguration}");

    public Task PackAsync(ProjectDescription project)
        => project.SdkStyle
            ? RunDotnetPackAsync(project)
            : RunNugetPackAsync(project);

    private async Task RunDotnetPackAsync(ProjectDescription project)
        => await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync(
                "dotnet",
                "pack",
                "--no-build",
                "--configuration",
                project.BuildConfiguration,
                "-property:OutputFileNamesWithoutVersion=true");

    private async Task RunNugetRestoreAsync(ProjectDescription project)
        => await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync("nuget", "restore");

    private async Task RunNugetPackAsync(ProjectDescription project)
    {
        var arguments = new List<string> { "nuget", "pack" };

        if (_msbuildExecutablePath != null)
        {
            arguments.Add("-MsBuildPath");
            arguments.Add(_msbuildExecutablePath.ParentDirectory.ToString());
        }

        arguments.Add("-Properties");
        arguments.Add($"Configuration={project.BuildConfiguration}");

        arguments.Add("-OutputDirectory");
        arguments.Add(project.PackageFilePath.ParentDirectory.ToString());

        arguments.Add("-OutputFileNamesWithoutVersion");

        await new SystemCommand()
            .WithWorkingDirectory(project.DirectoryPath)
            .RunAsync(arguments);
    }

    public class Provider : AsyncInitializingProvider<BuildSystem>
    {
        protected override async Task<BuildSystem> CreateInitializedAsync()
        {
            var msbuildExecutablePath = await ResolveToolboxRiderMsBuildExecutablePathAsync();
            return new BuildSystem(msbuildExecutablePath);
        }

        private static Task<AbsolutePath?> ResolveToolboxRiderMsBuildExecutablePathAsync()
        {
            var riderScriptFilePath = AbsolutePath.LocalAppDataDirectory
                / "JetBrains/Toolbox/scripts/Rider.cmd";

            if (!File.Exists(riderScriptFilePath.ToString()))
            {
                return Task.FromResult<AbsolutePath?>(null);
            }

            var riderScriptText = File.ReadAllText(riderScriptFilePath.ToString());
            var riderDirectoryPathMatch = Regex.Match(riderScriptText, @"\s([^\s]+?)\\bin\\rider64.exe\b");
            if (!riderDirectoryPathMatch.Success)
            {
                return Task.FromResult<AbsolutePath?>(null);
            }

            var result = new AbsolutePath(riderDirectoryPathMatch.Groups[1].Value)
                / "tools/MSBuild/Current/Bin/MSBuild.exe";

            return Task.FromResult<AbsolutePath?>(result);
        }
    }
}
