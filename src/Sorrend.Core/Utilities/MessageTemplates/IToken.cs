using System;
using System.Text;

namespace Sorrend.Core.Utilities.MessageTemplates
{
    internal interface IToken
    {
        void WriteTo(
            StringBuilder builder,
            object?[] arguments,
            IFormatProvider formatProvider);
    }
}
