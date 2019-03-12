using System;

namespace PlayingWithRabbitMQ.DemoElements
{
  public class DelaySettings
  {
    private static readonly Random _random = new Random();

    public int ProducerDelayMin { get; set; }
    public int ProducerDelayMax { get; set; }

    public int HandlerDelayMin { get; set; }
    public int HandlerDelayMax { get; set; }

    public int ProducerDelay => _random.Next(ProducerDelayMin, ProducerDelayMax);

    public int HandlerDelay => _random.Next(HandlerDelayMin, HandlerDelayMax);
  }
}
