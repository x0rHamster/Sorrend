using Sorrend.Core.VersionControl;

namespace Sorrend.IntegrationTests.Tools.Repositories
{
    public class CommitDescription
    {
        public string ShortHash { get; }

        public CommitHash Hash { get; }

        public CommitDescription(string hash, int shortHashLength)
        {
            Hash = new CommitHash(hash);
            ShortHash = Hash.GetShortHash(shortHashLength);
        }
    }
}
