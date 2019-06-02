using System;
using System.Runtime.Serialization;

namespace PlayingWithRabbitMQ.Queue.Exceptions
{
  public class BrokerFactoryException : QueuingException
  {
    public BrokerFactoryException()
    {
    }

    public BrokerFactoryException(string message) : base(message)
    {
    }

    public BrokerFactoryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected BrokerFactoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
