

using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Event.EventHandlers;
using EventBus.UnitTest.Event.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.UnitTest
{
    public class EventBusTests
    {
        private ServiceCollection _services;
        private IServiceProvider _serviceProvider;
        private Func<EventBusConfig> _config;
        private EventBusConfig _rabbitMQConfig;
        private EventBusConfig _azureConfig;

        [SetUp]
        public void Setup()
        {
            _rabbitMQConfig = new EventBusConfig
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "SellingBuddyTopicName",
                EventBusType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent",
            };

            _azureConfig = new EventBusConfig
            {
                EventBusConnectionString = "Endpoint=sb://techbuddyfurkan.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qkPu/kLnQt8U03s8qJO6axcfd0ONpuW7t+ASbPHk2xg=",
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "SellingBuddyTopicName",
                EventBusType = EventBusType.AzureServiceBus,
                EventNameSuffix = "IntegrationEvent",
            };

            _services = new ServiceCollection();
            _services.AddSingleton(sp => EventBusFactory.Create(_config(),sp));


            _serviceProvider = _services.BuildServiceProvider();
        }

        [Test]
        public void SubscribeEventOnRabbitMqTest()
        {
            _config = () => _rabbitMQConfig;
            var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            eventBus.Unsubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            Assert.Pass();
        }

        [Test]
        public void SubscribeEventOnAzureServiceBusTest()
        {
            _config = () => _azureConfig;
            var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            eventBus.Unsubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            Assert.Pass();
        }

        [Test]
        public void SendMessageToRabbitMqTest()
        {
            _config = () => _rabbitMQConfig;
            var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
            Assert.Pass();
        }

        [Test]
        public void SendMessageToAzureServiceBusTest()
        {
            _config = () => _azureConfig;
            var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
            Assert.Pass();
        }

        [Test]
        public void ConsumeOrderCreatedFromAzureServiceBusTest()
        {
            _config = () => _azureConfig;
            var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            Thread.Sleep(3000);
            Assert.Pass();
        }

        [Test]
        public void ConsumeOrderCreatedFromRabbitMQTest()
        {
            _config = () => _rabbitMQConfig;
            var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            Thread.Sleep(3000);
            Assert.Pass();
        }
    }
}