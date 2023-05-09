using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.Console.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Console.IntegrationEvents.EventHandlers
{
    public class OrderPaymentSuccessIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
    {
        private readonly ILogger<OrderPaymentSuccessIntegrationEventHandler> _logger;

        public OrderPaymentSuccessIntegrationEventHandler(ILogger<OrderPaymentSuccessIntegrationEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderPaymentSuccessIntegrationEvent @event)
        {
            //Send success notification (sms, email, push);

            _logger.LogInformation($"Payment success with order id: {@event.Id}");
            return Task.CompletedTask;
        }
    }

}
