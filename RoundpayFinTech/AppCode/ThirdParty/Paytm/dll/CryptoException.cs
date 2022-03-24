using System;

namespace paytm.exception
{
    public class CryptoException : Exception
    {
        public CryptoException()
        {
        }

        public CryptoException(string message) : base(message)
        {
        }

        public CryptoException(string message, Exception e) : base(message, e)
        {
        }

        public CryptoException(string format, params object[] args) : base(string.Format(format, args))
        {
        }

        public CryptoException(string format, Exception innerException, params object[] args) : base(string.Format(format, args), innerException)
        {
        }
    }
}
