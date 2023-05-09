using BasketService.Api.Core.Application.Repository;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;

namespace BasketService.Api.IntegrationEvents.EventHandlers
{
    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;
        public OrderCreatedIntegrationEventHandler(IBasketRepository basketRepository, ILogger<OrderCreatedIntegrationEventHandler> logger)
        {
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task Handle(OrderCreatedIntegrationEvent @event)
        {
            _logger.LogInformation($"----- Handling integration event: Event id : {@event.Id} at BasketService.Api");
            await _basketRepository.DeleteBasketAsync(@event.UserId);
        }
    }
}
