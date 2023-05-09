using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Console.IntegrationEvents.EventHandlers;
using NotificationService.Console.IntegrationEvents.Events;
using System.Reflection;

internal class Program
{
    private static ServiceCollection _services;
    private static void Main(string[] args)
    {
        _services = new ServiceCollection();
        ConfigureServices(_services);
        var sp = _services.BuildServiceProvider();
        IEventBus eventBus = sp.GetRequiredService<IEventBus>();
        eventBus.Subscribe<OrderPaymentSuccessIntegrationEvent, OrderPaymentSuccessIntegrationEventHandler>();
        eventBus.Subscribe<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();

        Console.WriteLine("Consumed to orderpaymentsuccess and orderpayment failed");
        Console.WriteLine("Application is running... Hit enter to stop orderpaymentfailed subscription.");
        Console.ReadLine();
        Console.WriteLine("Unsubscribing from orderpaymentfailed...");
        eventBus.Unsubscribe<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();
        Console.WriteLine("Unsubscribed from orderpaymentfailed...");
        Console.WriteLine("Hit enter to stop orderpaymentsucces subscription.");
        Console.ReadLine();
        Console.WriteLine("Unsubscribing from orderpaymentsucces...");
        eventBus.Unsubscribe<OrderPaymentSuccessIntegrationEvent, OrderPaymentSuccessIntegrationEventHandler>();
        Console.WriteLine("Unsubscribed from orderpaymentsucces...");
        eventBus.Dispose();

        Console.WriteLine("Hit enter to stop application.");
        Console.ReadLine();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddLogging(configure =>
        {
            configure.AddConsole();
        });

        services.AddTransient<OrderPaymentSuccessIntegrationEventHandler>();
        services.AddTransient<OrderPaymentFailedIntegrationEventHandler>();

        var config = new EventBusConfig
        {
            ConnectionRetryCount = 5,
            EventNameSuffix = "IntegrationEvent",
            SubscriberClientAppName = "NotificationService",
            IpAddress = "192.168.1.115:9092",
            EventBusType = EventBusType.Kafka
        };

        services.AddSingleton(sp => EventBusFactory.Create(config,sp));
    }
}