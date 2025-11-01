using System;
using System.Text;

namespace Sorrend.Core.Utilities.MessageTemplates
{
    internal class TextToken(string text) : IToken
    {
        public void WriteTo(
            StringBuilder builder,
            object?[] arguments,
            IFormatProvider formatProvider)
        {
            builder.Append(
                text
                    .Replace("{{", "{")
                    .Replace("}}", "}"));
        }
    }
}
