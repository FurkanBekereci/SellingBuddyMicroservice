using EventBus.AzureServiceBus;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Kafka;
using EventBus.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Factory
{
    public class EventBusFactory
    {
        public static IEventBus Create(EventBusConfig config, IServiceProvider serviceProvider)
        {
            return config.EventBusType switch
            {
                EventBusType.AzureServiceBus => new EventBusServiceBus(config, serviceProvider),
                EventBusType.RabbitMQ => new EventBusRabbitMQ(config, serviceProvider),
                EventBusType.Kafka => new EventBusKafka(config, serviceProvider),
                _ => throw new ArgumentException("Wrong event bus type. Please check event bus type.")
            };
        }
    }
}
