using System;
using JetBrains.Annotations;

namespace Sorrend.Core.Versions
{
    public class SemanticVersionIncrement
    {
        private int _majorReleaseCount;
        private int _minorReleaseCount;
        private int _patchReleaseCount;
        private SemanticVersioningReleaseType _preReleaseType;

        private int _counterPreReleaseIdentifierIncrement;

        private SemanticVersioningReleaseType LargestReleaseType
            => _majorReleaseCount > 0 ? SemanticVersioningReleaseType.Major
                : _minorReleaseCount > 0 ? SemanticVersioningReleaseType.Minor
                : _patchReleaseCount > 0 ? SemanticVersioningReleaseType.Patch
                : SemanticVersioningReleaseType.None;

        private bool HasReleases
            => LargestReleaseType != SemanticVersioningReleaseType.None;

        private bool HasPreReleases
            => _preReleaseType != SemanticVersioningReleaseType.None;

        public int GetMajorVersion(SemanticVersion currentVersion)
            => GetNormalVersionIdentifier(
                currentVersion.MajorVersion,
                currentVersion.IsPreRelease,
                SemanticVersioningReleaseType.Major,
                _majorReleaseCount);

        public int GetMinorVersion(SemanticVersion currentVersion)
            => GetNormalVersionIdentifier(
                currentVersion.MinorVersion,
                currentVersion.IsPreRelease,
                SemanticVersioningReleaseType.Minor,
                _minorReleaseCount);

        public int GetPatchVersion(SemanticVersion currentVersion)
            => GetNormalVersionIdentifier(
                currentVersion.PatchVersion,
                currentVersion.IsPreRelease,
                SemanticVersioningReleaseType.Patch,
                _patchReleaseCount);

        private int GetNormalVersionIdentifier(
            int currentValue,
            bool currentVersionIsPreRelease,
            SemanticVersioningReleaseType releaseType,
            int releaseCount)
        {
            try
            {
                var targetValue = currentValue;

                if (LargestReleaseType > releaseType)
                {
                    targetValue = 0;
                }

                checked
                {
                    targetValue += releaseCount;
                }

                if (HasReleases || !currentVersionIsPreRelease)
                {
                    if (_preReleaseType > releaseType)
                    {
                        targetValue = 0;
                    }

                    if (_preReleaseType == releaseType)
                    {
                        targetValue++;
                    }
                }

                return targetValue;
            }
            catch (OverflowException)
            {
                throw new UserOrientedException(
                    "Version increments cause the normal version number to overflow. Increase a higher-order normal version identifier.");
            }
        }

        [CanBeNull]
        public string GetPrefixPreReleaseIdentifier(
            SemanticVersion currentVersion,
            [CanBeNull] string currentValue,
            string defaultValue)
        {
            if (HasPreReleases)
            {
                return currentValue ?? defaultValue;
            }

            if (HasReleases)
            {
                return null;
            }

            if (currentVersion.IsPreRelease)
            {
                return currentValue ?? defaultValue;
            }

            return null;
        }

        public int? GetCounterPreReleaseIdentifier(SemanticVersion currentVersion, int? currentValue)
        {
            try
            {
                var targetValue = currentValue;

                if (currentVersion.IsPreRelease)
                {
                    targetValue = targetValue ?? 1;
                }

                if (HasReleases)
                {
                    targetValue = null;
                }

                if (targetValue == null && !HasPreReleases)
                {
                    return null;
                }

                if (_counterPreReleaseIdentifierIncrement > 0)
                {
                    targetValue = targetValue ?? 0;

                    checked
                    {
                        targetValue += _counterPreReleaseIdentifierIncrement;
                    }
                }

                return targetValue;
            }
            catch (OverflowException)
            {
                throw new UserOrientedException(
                    "Version increments cause the pre-release counter to overflow. Create a release version.");
            }
        }

        public void AddRelease(SemanticVersioningReleaseType type)
        {
            if (type == SemanticVersioningReleaseType.None)
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (LargestReleaseType > type)
            {
                return;
            }

            switch (type)
            {
                case SemanticVersioningReleaseType.Major:
                    _majorReleaseCount++;
                    break;

                case SemanticVersioningReleaseType.Minor:
                    _minorReleaseCount++;
                    break;

                case SemanticVersioningReleaseType.Patch:
                    _patchReleaseCount++;
                    break;
            }
        }

        public void AddPreRelease(SemanticVersioningReleaseType type)
        {
            if (type == SemanticVersioningReleaseType.None)
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (HasReleases)
            {
                if (type > LargestReleaseType)
                {
                    AddRelease(type);
                }

                return;
            }

            _counterPreReleaseIdentifierIncrement++;

            if (_preReleaseType < type)
            {
                _preReleaseType = type;
            }
        }
    }
}
