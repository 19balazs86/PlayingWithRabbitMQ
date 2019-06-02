using System;
using System.Runtime.Serialization;

namespace PlayingWithRabbitMQ.Queue.Exceptions
{
  public class MessageException : QueuingException
  {
    public MessageException()
    {
    }

    public MessageException(string message) : base(message)
    {
    }

    public MessageException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MessageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
