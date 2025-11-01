namespace Sorrend.IntegrationTests.Tools.Projects;

public static partial class ProjectTranslator
{
    public static string GetTargetFrameworkMoniker(TargetFramework targetFramework)
        => targetFramework switch
        {
            TargetFramework.NetFramework472 => "net472",
            TargetFramework.Net8 => "net8.0",

            _ => throw new ArgumentOutOfRangeException(
                nameof(targetFramework),
                targetFramework,
                null),
        };

    private static string GetTargetFrameworkVersion(TargetFramework targetFramework)
        => targetFramework switch
        {
            TargetFramework.NetFramework472 => "v4.7.2",

            _ => throw new ArgumentOutOfRangeException(
                nameof(targetFramework),
                targetFramework,
                null),
        };
}
