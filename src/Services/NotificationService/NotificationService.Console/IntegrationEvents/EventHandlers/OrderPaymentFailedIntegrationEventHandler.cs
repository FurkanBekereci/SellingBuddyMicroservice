using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.Console.IntegrationEvents.Events;

namespace NotificationService.Console.IntegrationEvents.EventHandlers
{
    public class OrderPaymentFailedIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
    {
        private readonly ILogger<OrderPaymentFailedIntegrationEvent> _logger;

        public OrderPaymentFailedIntegrationEventHandler(ILogger<OrderPaymentFailedIntegrationEvent> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderPaymentFailedIntegrationEvent @event)
        {
            //Send fail notification (sms, email, push);

            _logger.LogInformation($"Payment failed with order id: {@event.Id} and error message: {@event.ErrorMessage}");
            return Task.CompletedTask;
        }
    }

}
