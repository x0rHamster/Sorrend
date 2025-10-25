using System;
using System.Collections.Generic;
using Sorrend.Core.UserMessages;
using Sorrend.Core.Utilities;
using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.AssemblyVersioning
{
    public class CommitVersionParser
    {
        private const string CommitHashPreReleaseIdentifierPrefix = "r";
        private const int DefaultShortCommitHashLength = 12;

        public IReadOnlyCollection<SemanticVersion> ParseTags(IEnumerable<string> tags)
        {
            var candidates = new List<SemanticVersion>();

            foreach (var tag in tags)
            {
                using (UserMessageScopes.CommitTag(tag))
                {
                    if (tag.IsInteger())
                    {
                        continue;
                    }

                    var normalizedCommitTag = tag.TrimPrefix("v", StringComparison.OrdinalIgnoreCase);
                    var candidate = SemanticVersion.ParseOrDefault(normalizedCommitTag);

                    if (candidate != null)
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            return candidates;
        }

        public void ValidateHashPreReleaseIdentifier(string identifier, CommitHash commitHash)
        {
            if (!identifier.StartsWith(CommitHashPreReleaseIdentifierPrefix, StringComparison.Ordinal))
            {
                throw UserOrientedExceptions.CommitHashPreReleaseIdentifierWithoutPrefix(
                    identifier,
                    CommitHashPreReleaseIdentifierPrefix);
            }

            var identifierHash = identifier.Substring(CommitHashPreReleaseIdentifierPrefix.Length);
            CommitHash.Validate(identifierHash);

            if (!commitHash.StartsWith(identifierHash))
            {
                throw UserOrientedExceptions.CommitHashPreReleaseIdentifierDiffersFromCommit(
                    identifierHash,
                    commitHash);
            }
        }

        public string GetHashPreReleaseIdentifier(CommitHash hash)
        {
            return CommitHashPreReleaseIdentifierPrefix
                + hash.GetShortHash(DefaultShortCommitHashLength);
        }
    }
}
