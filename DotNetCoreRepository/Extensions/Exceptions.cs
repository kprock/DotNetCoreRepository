using System;

namespace DotNetCoreRepository.Extensions
{
    public class SerialNumberNotFoundException : Exception
    {
        public SerialNumberNotFoundException() : base() { }
        public SerialNumberNotFoundException(string message) : base(message) { }
        public SerialNumberNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    public class ValidStateCodeNotFoundException : Exception
    {
        public ValidStateCodeNotFoundException() { }
        public ValidStateCodeNotFoundException(string message) : base(message) { }
        public ValidStateCodeNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    public class BuyerEmailNotFoundException : Exception
    {
        public BuyerEmailNotFoundException() { }
        public BuyerEmailNotFoundException(string message) : base(message) { }
        public BuyerEmailNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    public class InvalidOrderTotalException : Exception
    {
        public InvalidOrderTotalException() { }
        public InvalidOrderTotalException(string message) : base(message) { }
        public InvalidOrderTotalException(string message, Exception inner) : base(message, inner) { }
    }
}
