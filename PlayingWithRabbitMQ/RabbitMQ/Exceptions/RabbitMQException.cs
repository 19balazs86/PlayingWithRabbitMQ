using System;
using System.Runtime.Serialization;

namespace PlayingWithRabbitMQ.RabbitMQ.Exceptions
{
  public class RabbitMQException : Exception
  {
    public RabbitMQException()
    {
    }

    public RabbitMQException(string message) : base(message)
    {
    }

    public RabbitMQException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected RabbitMQException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
