using System;
using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.AssemblyVersioning
{
    public class AssemblyVersionCalculation
    {
        private const int MaximumAssemblyNormalVersionIdentifier = 65534;

        private readonly SemanticVersioningScheme _versioningScheme;
        private readonly SemanticVersionIncrement _versionIncrement = new SemanticVersionIncrement();

        private SemanticVersion _baseVersion;
        private Commit _latestIncrementCommit;

        public bool HasResult
            => _baseVersion != null;

        public AssemblyVersionCalculation(SemanticVersioningScheme versioningScheme)
        {
            _versioningScheme = versioningScheme;
        }

        public void Add(Commit commit)
        {
            if (HasResult)
            {
                return;
            }

            var baseVersionFromCommit = _versioningScheme.FindBaseVersion(commit);
            if (baseVersionFromCommit != null)
            {
                ValidateAssemblyVersion(baseVersionFromCommit);

                _baseVersion = baseVersionFromCommit;
                _latestIncrementCommit = _latestIncrementCommit ?? commit;
                return;
            }

            _versioningScheme.UpdateVersionIncrement(_versionIncrement);
            _latestIncrementCommit = _latestIncrementCommit ?? commit;
        }

        public AssemblyVersionProperties GetResult()
        {
            var baseVersion = _baseVersion ?? _versioningScheme.GetInitialVersion();

            // TODO increment version should indicate that there were no commits
            var latestIncrementCommit = _latestIncrementCommit ?? throw new NotImplementedException();

            var latestIncrementVersion = _versioningScheme.GetIncrementVersion(
                baseVersion,
                _versionIncrement,
                latestIncrementCommit);

            ValidateAssemblyVersion(latestIncrementVersion);

            return new AssemblyVersionProperties(latestIncrementVersion);
        }

        private static void ValidateAssemblyVersion(SemanticVersion version)
        {
            ValidateAssemblyVersionIdentifier(version.MajorVersion);
            ValidateAssemblyVersionIdentifier(version.MinorVersion);
            ValidateAssemblyVersionIdentifier(version.PatchVersion);
        }

        private static void ValidateAssemblyVersionIdentifier(int identifier)
        {
            if (identifier > MaximumAssemblyNormalVersionIdentifier)
            {
                throw new UserOrientedException(
                    $"The normal version identifier {identifier} is greater than {MaximumAssemblyNormalVersionIdentifier}.");
            }
        }
    }
}
