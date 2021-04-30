using System;
using System.Runtime.Serialization;

namespace Stock.Server
{
    [Serializable]
    internal class StockServerException : Exception
    {
        public StockServerException()
        {
        }

        public StockServerException(string message) : base(message)
        {
        }

        public StockServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StockServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}