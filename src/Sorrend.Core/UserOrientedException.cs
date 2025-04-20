using System;

namespace Sorrend.Core
{
    public class UserOrientedException : Exception
    {
        public UserOrientedException()
        {
        }

        public UserOrientedException(string message)
            : base(message)
        {
        }

        public UserOrientedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
