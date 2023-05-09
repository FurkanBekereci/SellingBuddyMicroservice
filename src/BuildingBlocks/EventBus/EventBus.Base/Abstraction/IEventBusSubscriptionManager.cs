using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    /// <summary>
    /// Event bus'ın subscribe olduğu eventleri handle eden interfacedir.
    /// Örneğin, bunu implement eden manager, bir in-memory yada bir database yapısına sahip olabilir.
    /// </summary>
    public interface IEventBusSubscriptionManager
    {

        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;
        void AddSubscription<T, THandler>() where T: IntegrationEvent where THandler : IIntegrationEventHandler<T>;
        void RemoveSubscription<T, THandler>() where T: IntegrationEvent where THandler : IIntegrationEventHandler<T>;
        bool HasSubscriptionForEvent<T>() where T: IntegrationEvent;
        bool HasSubscriptionForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        string GetEventKey<T>();

    }
}

//Notlar : Rabbit mq ve azure service buslar yapılırken daha da anlaşılır olacak.
