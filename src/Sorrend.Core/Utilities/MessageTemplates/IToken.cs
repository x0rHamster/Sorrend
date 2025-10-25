using System;
using System.Text;
using JetBrains.Annotations;

namespace Sorrend.Core.Utilities.MessageTemplates
{
    internal interface IToken
    {
        void WriteTo(
            StringBuilder builder,
            [ItemCanBeNull] object[] arguments,
            IFormatProvider formatProvider);
    }
}
