using System;
using Sorrend.Core.Utilities;
using Sorrend.Core.Versions;

namespace Sorrend.Core.AssemblyVersioning
{
    public class AssemblyVersionProperties(SemanticVersion latestIncrementVersion)
    {
        public string Version { get; }
            = latestIncrementVersion.NormalVersion
            + latestIncrementVersion.PreReleaseSuffix;

        public string VersionPrefix { get; }
            = latestIncrementVersion.NormalVersion;

        public string VersionSuffix { get; }
            = latestIncrementVersion.PreReleaseSuffix
                .TrimPrefix("-", StringComparison.Ordinal);

        public string AssemblyVersion { get; }
            = latestIncrementVersion.MajorVersion + ".0.0.0";

        public string FileVersion { get; }
            = latestIncrementVersion.NormalVersion + ".0";

        public string InformationalVersion { get; }
            = latestIncrementVersion.NormalVersion
            + latestIncrementVersion.PreReleaseSuffix
            + latestIncrementVersion.BuildMetadataSuffix;

        // see https://learn.microsoft.com/en-us/nuget/reference/nuspec#replacement-tokens
        public string PackageVersion
            => InformationalVersion;
    }
}
