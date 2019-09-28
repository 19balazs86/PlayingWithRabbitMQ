using System;
using PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Queue;
using PlayingWithRabbitMQ.Queue.Azure.ServiceBus.Topic;
using PlayingWithRabbitMQ.Queue.RabbitMQ;

namespace PlayingWithRabbitMQ.DemoElements.Messages
{
  // The login message coming from the user service. The statistics service consume it.
  [MessageSettings(
    exchangeName: "service.user",
    exchangeType: ExchangeType.Direct,
    routeKey: "login",
    queueName: "user.login.statistics",
    deadLetterQueue: "user.login.statistics.sink")]
  [AzureQueue(queueName: "user.login", maxConcurrentCalls: 5)]
  [AzureTopic(topic: "service.user", subscription: "user.login.statistics", routeKey: "login")]
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
