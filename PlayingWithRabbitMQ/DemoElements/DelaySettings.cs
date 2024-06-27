namespace PlayingWithRabbitMQ.DemoElements;

public class DelaySettings
{
    public int ProducerDelayMin { get; init; }
    public int ProducerDelayMax { get; init; }

    public int HandlerDelayMin { get; init; }
    public int HandlerDelayMax { get; init; }

    public int ProducerDelay => Random.Shared.Next(ProducerDelayMin, ProducerDelayMax);

    public int HandlerDelay => Random.Shared.Next(HandlerDelayMin, HandlerDelayMax);
}
