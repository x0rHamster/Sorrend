using System;
using System.Text;

namespace Sorrend.Core.Utilities.MessageTemplates
{
    internal class TextToken : IToken
    {
        private readonly string _text;

        public TextToken(string text)
        {
            _text = text;
        }

        public void WriteTo(
            StringBuilder builder,
            object[] arguments,
            IFormatProvider formatProvider)
        {
            builder.Append(
                _text
                    .Replace("{{", "{")
                    .Replace("}}", "}"));
        }
    }
}
