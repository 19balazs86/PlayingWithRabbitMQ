using System;
using System.Runtime.Serialization;

namespace PlayingWithRabbitMQ.Queue.Exceptions
{
  public class QueueFactoryException : RabbitMQException
  {
    public QueueFactoryException()
    {
    }

    public QueueFactoryException(string message) : base(message)
    {
    }

    public QueueFactoryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected QueueFactoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
