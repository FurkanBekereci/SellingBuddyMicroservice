using EventBus.Base.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EventBus.Base.Extensions
{
    public static class EventHandlerRegistration
    {
        public static IServiceCollection ConfigureEventHandlers(this IServiceCollection services, Func<Type, IServiceCollection> registerWith, Assembly caller = null)
        {
            var assembly = caller ?? Assembly.GetCallingAssembly();
            var allTypes = assembly.GetTypes();
            var typesImplementIIntegrationEventHandler = allTypes.Where(x => x.GetInterfaces().Any(i => i.IsInterface && i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)));

            foreach (var type in typesImplementIIntegrationEventHandler)
            {
                registerWith(type);
            }

            return services;
        }
    }
}
