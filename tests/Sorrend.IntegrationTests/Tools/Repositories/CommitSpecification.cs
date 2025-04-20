using System;
using System.Globalization;

namespace Sorrend.IntegrationTests.Tools.Repositories
{
    public class CommitSpecification
    {
        public DateTimeOffset? AuthorDateTime { get; set; }

        public CommitSpecification WithAuthorDateTime(string value)
        {
            AuthorDateTime = DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
            return this;
        }
    }
}
