using System;
using PlayingWithRabbitMQ.Queue.RabbitMQ;

namespace PlayingWithRabbitMQ.DemoElements.Messages
{
  // The purchase message coming from the order service. The shipping service consume it.
  [MessageSettings(
    exchangeName: "service.order",
    exchangeType: ExchangeType.Direct,
    routeKey: "purchase",
    queueName: "order.purchase.shipping",
    deadLetterQueue: "order.purchase.shipping.sink")]
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
