using System;
using System.Reflection;

namespace PlayingWithRabbitMQ.Queue
{
  public interface IAttributeProvider<A> where A : Attribute
  {
    A GetAttributeFor<T>() where T : class;
  }

  public class SimpleAttributeProvider<A> : IAttributeProvider<A> where A : Attribute
  {
    public virtual A GetAttributeFor<T>() where T : class
      => InnerClassProvider<T>.Value;

    private static class InnerClassProvider<T>
    {
      // Benchmark for this trick: https://mariusgundersen.net/type-dictionary-trick
      public static readonly A Value
        = typeof(T).GetCustomAttribute<A>();
    }
  }
}
