using System;
using Sorrend.Core.VersionControl;
using Sorrend.Core.Versions;

namespace Sorrend.Core.UserMessages
{
    internal static class UserMessageScopes
    {
        public static IDisposable Commit(Commit commit)
            => UserOrientedExceptionFactory.BeginScope(
                "Commit {CommitHash}",
                commit.Hash);

        public static IDisposable CommitTag(string tag)
            => UserOrientedExceptionFactory.BeginScope(
                "Commit tag {CommitTag}",
                tag);

        public static IDisposable BaseVersion(SemanticVersion version)
            => UserOrientedExceptionFactory.BeginScope(
                "Base version {BaseVersion}",
                version);

        public static IDisposable Version(SemanticVersion version)
            => UserOrientedExceptionFactory.BeginScope(
                "Version {Version}",
                version);
    }
}
