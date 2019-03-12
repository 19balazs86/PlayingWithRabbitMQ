using System;
using System.Runtime.Serialization;

namespace PlayingWithRabbitMQ.Queue.Exceptions
{
  public class ProducerException : QueueBaseException
  {
    public ProducerException()
    {
    }

    public ProducerException(string message) : base(message)
    {
    }

    public ProducerException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ProducerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
