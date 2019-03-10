﻿using Microsoft.Extensions.Configuration;

namespace PlayingWithRabbitMQ
{
  public static class ConfigurationExtensions
  {
    public static T BindTo<T>(this IConfiguration configuration, string key) where T : class, new()
    {
      T bindingObject = new T();

      configuration.GetSection(key).Bind(bindingObject);

      return bindingObject;
    }
  }
}