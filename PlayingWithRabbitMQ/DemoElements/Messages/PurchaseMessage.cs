using System;

namespace PlayingWithRabbitMQ.DemoElements.Messages
{
  public class PurchaseMessage
  {
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public DateTime Date { get; set; }

    public PurchaseMessage()
    {
      Id     = Guid.NewGuid();
      ItemId = Guid.NewGuid();
      Date   = DateTime.Now;
    }
  }
}
