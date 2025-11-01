using System;
using System.Collections.Generic;

namespace Sorrend.Core.VersionControl
{
    public record Commit(
        CommitHash Hash,
        DateTimeOffset AuthorDateTime,
        IReadOnlyCollection<string> Tags);
}
