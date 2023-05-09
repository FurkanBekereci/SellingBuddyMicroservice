using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    /// <summary>
    /// Gelen iç haberleşme eventlerinin handle edilmesine olanak sağlayan event handler interface'idir.
    /// IEventBus ile aynı mantık üzerinden gidilir. Yani concrete bir event handler implementasyonuna
    /// bağlı kalınmaması için sistem bu event handler üzerine inşa edilmiştir.
    /// </summary>
    /// <typeparam name="TIntegrationEvent"></typeparam>
    public interface IIntegrationEventHandler<TIntegrationEvent> : IntegrationEventHandler where TIntegrationEvent : IntegrationEvent
    {

        Task Handle(TIntegrationEvent @event);
    }

    /// <summary>
    /// İç haberleşme event handlerleri için imza interface'idir. Bir classın ya da interface'in IntegrationEventHandler olması için 
    /// bu interface implement edilmelidir.
    /// </summary>
    public interface IntegrationEventHandler
    {

    }
}
