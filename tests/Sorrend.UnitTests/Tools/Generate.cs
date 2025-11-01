using System.Globalization;
using Sorrend.Core.VersionControl;

namespace Sorrend.UnitTests.Tools;

public static class Generate
{
    private static string CommitHash()
        => Guid.NewGuid().ToString("N");

    public static Commit Commit(
        string? hash = null,
        string? date = null,
        string? tag = null,
        IReadOnlyCollection<string>? tags = null)
    {
        hash ??= CommitHash();

        date ??= "1999-12-31T23:59:59-08:00";

        if (tag != null)
        {
            tags ??= [tag];
        }

        tags ??= [];

        return new Commit(
            new CommitHash(hash),
            DateTimeOffset.Parse(date, CultureInfo.InvariantCulture),
            tags);
    }
}
