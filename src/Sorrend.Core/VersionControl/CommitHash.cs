using System;
using System.Diagnostics.CodeAnalysis;
using Sorrend.Core.UserMessages;
using Sorrend.Core.Utilities;

namespace Sorrend.Core.VersionControl
{
    public sealed class CommitHash : IEquatable<CommitHash>
    {
        private const int MinimumLength = 4;

        private readonly string _value;

        [SuppressMessage(
            "Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Popular VCSes output commit hashes in lowercase")]
        public CommitHash(string value)
        {
            _value = value.ToLowerInvariant();
        }

        public static void Validate(string value)
        {
            if (!value.IsHexNumber())
            {
                throw UserOrientedExceptions.CommitHashMustBeHexadecimal(value);
            }

            if (value.Length < MinimumLength)
            {
                throw UserOrientedExceptions.TooShortCommitHash(value, MinimumLength);
            }
        }

        public string GetShortHash(int length)
            => _value.Substring(0, length);

        public bool StartsWith(string prefix)
            => _value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        public bool Equals(CommitHash other)
            => _value.Equals(other?._value, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => Equals(obj as CommitHash);

        public override int GetHashCode()
            => _value.GetHashCode();

        public override string ToString()
            => _value;
    }
}
