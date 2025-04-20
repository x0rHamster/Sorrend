using System;
using Sorrend.Core.Utilities;
using Sorrend.Core.Versions;

namespace Sorrend.Core.AssemblyVersioning
{
    public class AssemblyVersionProperties
    {
        private readonly SemanticVersion _latestIncrementVersion;

        public string Version
            => _latestIncrementVersion.NormalVersion
                + _latestIncrementVersion.PreReleaseSuffix;

        public string VersionPrefix
            => _latestIncrementVersion.NormalVersion;

        public string VersionSuffix
            => _latestIncrementVersion.PreReleaseSuffix
                .TrimPrefix("-", StringComparison.Ordinal);

        public string AssemblyVersion
            => _latestIncrementVersion.MajorVersion + ".0.0.0";

        public string FileVersion
            => _latestIncrementVersion.NormalVersion + ".0";

        public string InformationalVersion
            => _latestIncrementVersion.NormalVersion
                + _latestIncrementVersion.PreReleaseSuffix
                + _latestIncrementVersion.BuildMetadataSuffix;

        // see https://learn.microsoft.com/en-us/nuget/reference/nuspec#replacement-tokens
        public string PackageVersion
            => InformationalVersion;

        public AssemblyVersionProperties(SemanticVersion latestIncrementVersion)
        {
            _latestIncrementVersion = latestIncrementVersion;
        }
    }
}
