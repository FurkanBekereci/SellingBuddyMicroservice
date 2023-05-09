using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
    public class EventBusKafka : BaseEventBus
    {
        private readonly KafkaClient _kafkaClient;

        public EventBusKafka(EventBusConfig eventBusConfig, IServiceProvider serviceProvider) : base(eventBusConfig, serviceProvider)
        {
            _kafkaClient = new KafkaClient(eventBusConfig.IpAddress, eventBusConfig.SubscriberClientAppName);
            _kafkaClient.OnErrorHandled += KafkaOnErrorHandled;
            _kafkaClient.OnMessageHandled += KafkaOnMessageHandled;
        }

        public override void Publish(IntegrationEvent @event)
        {
            var typeName = @event.GetType().Name;
            var eventName = ProcessEventName(typeName);

            _kafkaClient.PublishMessage($"{eventName}", @event);
        }

        public override void Subscribe<T, THandler>()
        {
            if (SubscriptionManager.HasSubscriptionForEvent<T>()) return;

            var typeName = typeof(T).Name;
            var eventName = ProcessEventName(typeName);
            var subscriptionName = GetSubName(typeName);

            _kafkaClient.CreateTopic($"{eventName}").GetAwaiter().GetResult();
            _kafkaClient.StartConsuming<T>($"{eventName}", subscriptionName);

            SubscriptionManager.AddSubscription<T, THandler>();

        }

        private bool KafkaOnMessageHandled(string eventName, string message)
        {
            return ProcessEvent(ProcessEventName(eventName), message).GetAwaiter().GetResult();
        }

        private void KafkaOnErrorHandled(Exception ex)
        {
            Console.WriteLine($"Error handled when reading data from kafka server: Message {ex.Message}");
        }

        public override void Unsubscribe<T, THandler>()
        {
            var typeName = typeof(T).Name;
            var eventName = ProcessEventName(typeName);

            _kafkaClient.StopConsuming($"{eventName}");

            SubscriptionManager.RemoveSubscription<T, THandler>();
        }
    }
}
