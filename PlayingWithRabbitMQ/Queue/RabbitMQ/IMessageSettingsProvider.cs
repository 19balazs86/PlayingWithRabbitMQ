using System.Reflection;

namespace PlayingWithRabbitMQ.Queue.RabbitMQ
{
  public interface IMessageSettingsProvider
  {
    MessageSettingsAttribute GetMessageSettingsFor<T>() where T : class;
  }

  public class SimpleMessageSettingsProvider : IMessageSettingsProvider
  {
    public virtual MessageSettingsAttribute GetMessageSettingsFor<T>() where T : class
      => typeof(T).GetCustomAttribute<MessageSettingsAttribute>();
  }
}
