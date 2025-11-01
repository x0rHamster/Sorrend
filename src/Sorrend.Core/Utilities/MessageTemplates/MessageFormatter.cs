using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sorrend.Core.Utilities.MessageTemplates
{
    public static class MessageFormatter
    {
        private static readonly Regex PlaceholderRegex
            = new(@"(?<!\{)\{([0-9a-zA-Z_]+(?:,-?\d+)?(?::[^\}]+)?)\}(?!\})");

        public static string Format(
            string messageTemplate,
            object?[] arguments,
            IFormatProvider formatProvider)
        {
            var tokens = Tokenize(messageTemplate);
            PlaceholderToken.RecalculateIndexes(tokens);
            return Format(tokens, arguments, formatProvider);
        }

        private static string Format(
            IEnumerable<IToken> tokens,
            object?[] arguments,
            IFormatProvider formatProvider)
        {
            var builder = new StringBuilder();

            foreach (var token in tokens)
            {
                token.WriteTo(builder, arguments, formatProvider);
            }

            return builder.ToString();
        }

        private static IToken[] Tokenize(string messageTemplate)
            => PlaceholderRegex.Split(messageTemplate)
                .Select<string, IToken>(
                    (x, i) => IsEven(i)
                        ? new TextToken(x)
                        : new PlaceholderToken(x))
                .ToArray();

        [SuppressMessage(
            "Major Code Smell",
            "S109:Magic numbers should not be used",
            Justification = "This method is too trivial to extract a constant")]
        private static bool IsEven(int value)
            => value % 2 == 0;
    }
}
