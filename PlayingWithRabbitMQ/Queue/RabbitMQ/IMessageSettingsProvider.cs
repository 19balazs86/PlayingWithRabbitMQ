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
      => InnerClassProvider<T>.Value;

    private static class InnerClassProvider<T>
    {
      // Benchmark for this trick: https://mariusgundersen.net/type-dictionary-trick
      public static readonly MessageSettingsAttribute Value
        = typeof(T).GetCustomAttribute<MessageSettingsAttribute>();
    }
  }
}
