using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace IdentityService.Api.Extensions.Registrations
{
    public static class ConsulRegistration
    {
        public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = configuration["ConsulConfig:Address"];
                consulConfig.Address = new Uri(address);
            }));

            return services;
        }

        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
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
                ID = $"IdentityService",
                Name = "IdentityService",
                Address = $"{uri.Host}",
                Port = uri.Port,
                Tags = new[] { "Identity Service", "Identity", "IdentityService", "Identities", "identity_service" }
            };

            logger.LogInformation("Registering with Consul");

            consulClient.Agent.ServiceDeregister(registration.ID).GetAwaiter().GetResult();
            consulClient.Agent.ServiceRegister(registration).GetAwaiter().GetResult();

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Deregistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).GetAwaiter().GetResult();
            });

            return app;
        }
    }
}
