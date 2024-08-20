using System;

namespace Sorrend.IntegrationTests.Tools.Projects
{
    public static partial class ProjectTranslator
    {
        public static string GetTargetFrameworkMoniker(TargetFramework targetFramework)
        {
            switch (targetFramework)
            {
                case TargetFramework.NetFramework472:
                    return "net472";

                case TargetFramework.Net8:
                    return "net8.0";

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(targetFramework),
                        targetFramework,
                        null);
            }
        }

        private static string GetTargetFrameworkVersion(TargetFramework targetFramework)
        {
            switch (targetFramework)
            {
                case TargetFramework.NetFramework472:
                    return "v4.7.2";

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(targetFramework),
                        targetFramework,
                        null);
            }
        }
    }
}
