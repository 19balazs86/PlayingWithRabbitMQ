using System;

namespace PlayingWithRabbitMQ.DemoElements.Messages
{
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
