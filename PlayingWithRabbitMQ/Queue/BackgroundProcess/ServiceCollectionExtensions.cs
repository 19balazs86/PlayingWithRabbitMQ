using PlayingWithRabbitMQ.Queue.BackgroundProcess;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static readonly Type _messageHandlerType = typeof(IMessageHandler<>);

    public static IServiceCollection AddMessageHandlers(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        return services.AddMessageHandlers(Assembly.GetExecutingAssembly(), serviceLifetime);
    }

    public static IServiceCollection AddMessageHandlers(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        // Looking for non-generic interface implementations
        // ...Where(x => typeof(INameOfInterface).IsAssignableFrom(x) && ...)

        foreach (TypeInfo definedType in assembly.DefinedTypes.Where(x => x is { IsClass: true, IsInterface: false, IsAbstract: false }))
        {
            Type implementedInterface = definedType.ImplementedInterfaces
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == _messageHandlerType);

            if (implementedInterface is not null)
            {
                var ServiceDescriptor = new ServiceDescriptor(implementedInterface, definedType, serviceLifetime);

                services.Add(ServiceDescriptor);
            }
        }

        return services;
    }
}
