using System;

namespace Sorrend.Core.Utilities
{
    public class TypeArgumentException : Exception
    {
        public TypeArgumentException()
        {
        }

        public TypeArgumentException(string message)
            : base(message)
        {
        }

        public TypeArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
