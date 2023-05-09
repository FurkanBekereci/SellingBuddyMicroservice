using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base
{
    public class EventBusConfig
    {
        public int ConnectionRetryCount { get; set; } = 5;
        public string DefaultTopicName { get; set; } = "SellingBuddyEventBus";
        public string EventBusConnectionString { get; set; } = string.Empty;
        public string SubscriberClientAppName { get; set; } = string.Empty;
        public string EventNamePrefix { get; set; } = string.Empty;
        public string EventNameSuffix { get; set; } = "IntegrationEvent";
        public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ;
        public string IpAddress { get; set; } = string.Empty;
        public object Connection { get; set; }

        public bool DeleteEventPrefix => !string.IsNullOrEmpty(EventNamePrefix);
        public bool DeleteEventSuffix => !string.IsNullOrEmpty(EventNameSuffix);
    }

    public enum EventBusType
    {
        RabbitMQ = 0,
        AzureServiceBus = 1,
        Kafka = 2,

    }

    //public interface EventBusConnectionParameters{

    //    protected object GetConnection();
    //}

    //public class RabbitMqConnectionParameters : EventBusConnectionParameters
    //{
    //    object EventBusConnectionParameters.GetConnection()
    //    {
    //        return 
    //    }
    //}

    //public class AzureServiceBusConnectionParameters : EventBusConnectionParameters
    //{
    //    public object GetConnection()
    //    {
    //        return 
    //    }
    //}

    //public static class EventBusConnectionParametersExtensions
    //{
    //    public static Func<T, EventBusConnectionParameters> GetEventBusConnectionParameters<T>(params object[] parameters) where T : class, EventBusConnectionParameters
    //    {

    //        return null;
    //    } 

    //}

}
