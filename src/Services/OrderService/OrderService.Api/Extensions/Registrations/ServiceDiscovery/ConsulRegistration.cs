using Consul;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace OrderService.Api.Extensions.Registrations.ServiceDiscovery
{
    public static class ConsulRegistration
    {
        public static IServiceCollection AddServiceDiscoveryRegistration(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddSingleton<IConsulClient>(_ => new ConsulClient(config =>
            {
                var address = configuration["ConsulConfig:Address"];
                config.Address = new Uri(address);
            }));

            return services;
        }

        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var loggingFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggingFactory.CreateLogger<IApplicationBuilder>();

            var features = app.ServerFeatures as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.FirstOrDefault();

            var uri = new Uri(address);
            var registration = new AgentServiceRegistration()
            {
                ID = $"OrderService",
                Name = "OrderService",
                Address = $"{uri.Host}",
                Port = uri.Port,
                Tags = new[] { "Order Service", "Order", "OrderService", "Orders", "order_service" }
            };
   
            logger.LogInformation("Registering with Consul");


            try
            {

                consulClient.Agent.ServiceDeregister(registration.ID).GetAwaiter().GetResult();
                consulClient.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                logger.LogWarning("Consul says : Be sure if the connection exists;");
            }

            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    logger.LogInformation("Deregistering from Consul");
                    consulClient.Agent.ServiceDeregister(registration.ID).GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                    logger.LogWarning("Consul says : Be sure if the connection exists;");
                }
                
            });

            return app;

        }
    }
}
