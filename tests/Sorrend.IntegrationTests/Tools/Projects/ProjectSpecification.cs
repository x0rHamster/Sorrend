using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sorrend.IntegrationTests.Tools.Packages;

namespace Sorrend.IntegrationTests.Tools.Projects
{
    public class ProjectSpecification
    {
        public bool? SdkStyle { get; set; }

        public List<TargetFramework> TargetFrameworks { get; } = new List<TargetFramework>();

        public List<PackageDescription> PackageReferences { get; } = new List<PackageDescription>();

        public bool EffectiveSdkStyle
            => SdkStyle ?? true;

        public TargetFramework EffectiveTargetFramework
        {
            get
            {
                if (TargetFrameworks.Count > 1)
                {
                    throw new InvalidOperationException(
                        $"The property \"{nameof(TargetFrameworks)}\" contains multiple values, use it instead.");
                }

                if (TargetFrameworks.Count == 1)
                {
                    return TargetFrameworks[0];
                }

                return EffectiveSdkStyle
                    ? TargetFramework.Net8
                    : TargetFramework.NetFramework472;
            }
        }

        public ProjectSpecification WithSdkStyle(bool sdkStyle = true)
        {
            SdkStyle = sdkStyle;
            return this;
        }

        [UsedImplicitly]
        [Obsolete("This method requires at least one argument.", true)]
        public ProjectSpecification WithTargetFrameworks()
            => throw new NotSupportedException("This method requires at least one argument.");

        public ProjectSpecification WithTargetFrameworks(params TargetFramework[] targetFrameworks)
        {
            TargetFrameworks.AddRange(targetFrameworks);
            return this;
        }

        public ProjectSpecification WithReference(PackageDescription package)
        {
            PackageReferences.Add(package);
            return this;
        }
    }
}
