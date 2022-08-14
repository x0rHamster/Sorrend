namespace Sorrend.IntegrationTests.Tools.Repositories
{
    public class CommitDescription
    {
        public string ShortHash { get; }

        public CommitDescription(string shortHash)
        {
            ShortHash = shortHash;
        }
    }
}
