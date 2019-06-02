using System;
using System.Runtime.Serialization;

namespace PlayingWithRabbitMQ.Queue.Exceptions
{
  public class QueuingException : Exception
  {
    public QueuingException()
    {
    }

    public QueuingException(string message) : base(message)
    {
    }

    public QueuingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected QueuingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
