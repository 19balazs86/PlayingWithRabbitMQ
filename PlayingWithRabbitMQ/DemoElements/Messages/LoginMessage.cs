using System;
using PlayingWithRabbitMQ.Queue;

namespace PlayingWithRabbitMQ.DemoElements.Messages
{
  // The login message coming from the user service. The statistics service consume it.
  [QueueMessage(
    exchangeName: "service.user",
    exchangeType: ExchangeType.Direct,
    routeKey: "login",
    queueName: "user.login.statistics",
    deadLetterQueue: "user.login.statistics.sink")]
  public class LoginMessage
  {
    public Guid UserId { get; set; }
    public DateTime LoginTime { get; set; }

    public LoginMessage()
    {
      UserId    = Guid.NewGuid();
      LoginTime = DateTime.Now;
    }
  }
}
