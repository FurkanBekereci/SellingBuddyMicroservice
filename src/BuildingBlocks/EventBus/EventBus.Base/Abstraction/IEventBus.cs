using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    /// <summary>
    /// Herhangi bir concrete message bus yapısına bağlı kalmamak amacıyla yazılmıştır
    /// Mikroservislerin birbirleri arasındaki veri alışverişini hangi diğer mikroservislerle yapacağı
    /// bu interface merkezinde belirlenir ve bu interface'i implement eden herhangi bir 
    /// concrete message bus üzerinden yapılır.
    /// </summary>
    public interface IEventBus : IDisposable
    {
        void Publish(IntegrationEvent @event);

        void Subscribe<T, THandler>() where T : IntegrationEvent where THandler : IIntegrationEventHandler<T>;

        void Unsubscribe<T, THandler>() where T : IntegrationEvent where THandler : IIntegrationEventHandler<T>;
    }
}
