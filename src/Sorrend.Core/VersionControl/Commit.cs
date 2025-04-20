using System;
using System.Collections.Generic;

namespace Sorrend.Core.VersionControl
{
    public class Commit
    {
        public CommitHash Hash { get; }

        public DateTimeOffset AuthorDateTime { get; }

        public IReadOnlyCollection<string> Tags { get; }

        public Commit(
            CommitHash hash,
            DateTimeOffset authorDateTime,
            IReadOnlyCollection<string> tags)
        {
            Hash = hash;
            AuthorDateTime = authorDateTime;
            Tags = tags;
        }
    }
}
