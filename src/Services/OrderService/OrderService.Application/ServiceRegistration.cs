using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace OrderService.Application
{
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assembly);
            services.AddMediatR(conf => conf.RegisterServicesFromAssembly(assembly));

            return services;
        }
    }
}
