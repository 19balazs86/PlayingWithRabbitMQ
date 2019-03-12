using System;
using System.Runtime.Serialization;

namespace PlayingWithRabbitMQ.Queue.Exceptions
{
  public class QueueBaseException : Exception
  {
    public QueueBaseException()
    {
    }

    public QueueBaseException(string message) : base(message)
    {
    }

    public QueueBaseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected QueueBaseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
