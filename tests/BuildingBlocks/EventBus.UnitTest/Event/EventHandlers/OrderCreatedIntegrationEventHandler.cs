using EventBus.Base.Abstraction;
using EventBus.UnitTest.Event.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.UnitTest.Event.EventHandlers
{
    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        public Task Handle(OrderCreatedIntegrationEvent @event)
        {
            Console.WriteLine($"Handle worked with event id : {@event.Id}");
            Debug.WriteLine($"Handle worked with event id : {@event.Id}");
            return Task.CompletedTask;
        }
    }
}
