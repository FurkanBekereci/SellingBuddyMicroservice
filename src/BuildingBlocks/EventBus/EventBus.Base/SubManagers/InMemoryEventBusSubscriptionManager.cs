using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.SubManagers
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public Func<string, string> eventNameGetter;

        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter)
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            this.eventNameGetter = eventNameGetter;
        }

        public bool IsEmpty => !_handlers.Keys.Any();

        public void AddSubscription<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            var eventKey = GetEventKey<T>();

            AddSubscription(typeof(THandler), eventKey);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        private void AddSubscription(Type handlerType, string eventKey)
        {
            if (!HasSubscriptionForEvent(eventKey))
            {
                _handlers.Add(eventKey, new List<SubscriptionInfo>());
            }

            if (_handlers[eventKey].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler type {handlerType.Name} already registered for '{eventKey}'", nameof(handlerType));
            }

            _handlers[eventKey].Add(SubscriptionInfo.Typed(handlerType));
        }

        public void Clear() => _handlers.Clear();

        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;
            return eventNameGetter(eventName);
        }

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var eventKey = GetEventKey<T>();
            return GetHandlersForEvent(eventKey);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        public bool HasSubscriptionForEvent<T>() where T : IntegrationEvent
        {
            var eventKey = GetEventKey<T>();

            return HasSubscriptionForEvent(eventKey);
        }

        public bool HasSubscriptionForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public void RemoveSubscription<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionToRemove<T, THandler>();
            var eventKey = GetEventKey<T>();
            RemoveHandler(eventKey, handlerToRemove);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {

            var eventKey = GetEventKey<T>();

            if (!HasSubscriptionForEvent(eventKey))
            {
                return null;
            }

            return _handlers[eventKey].SingleOrDefault(s => s.HandlerType == typeof(THandler));

        }

        private void RemoveHandler(string eventKey, SubscriptionInfo handlerToRemove)
        {
            if (handlerToRemove == null) return;

            _handlers[eventKey].Remove(handlerToRemove);

            if (_handlers[eventKey].Any()) return;

            _handlers.Remove(eventKey);
            var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventKey);

            if (eventType != null) 
                _eventTypes.Remove(eventType);

            RaiseOnEventRemoved(eventKey);

        }

        private void RaiseOnEventRemoved(string eventKey)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventKey);
        }
    }
}
