using EventBus.Base.Abstraction;
using MediatR;
using OrderService.Api.IntegrationEvents.Events;
using OrderService.Application.Features.Commands.CreateOrder;
using OrderService.Application.Interfaces.Repositories;

namespace OrderService.Api.IntegrationEvents.EventHandlers
{
    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        private IMediator _mediator;
        private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;
        private readonly IServiceProvider _services;

        public OrderCreatedIntegrationEventHandler(IServiceProvider services,ILogger<OrderCreatedIntegrationEventHandler> logger)
        {
            //_mediator = mediator;
            _services = services;
            _logger = logger;
        }

        public async Task Handle(OrderCreatedIntegrationEvent @event)
        {
            _logger.LogInformation("Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent}). ", @event.Id, typeof(Program).Namespace, @event);

            var createOrderCommand = new CreateOrderCommand(
                @event.Basket.Items,
                @event.UserId,
                @event.UserName,
                @event.City,
                @event.Street,
                @event.State,
                @event.Country,
                @event.ZipCode,
                @event.CardNumber,
                @event.CardHolderName,
                @event.CardExpiration,
                @event.CardSecurityNumber,
                @event.CardTypeId
                );
            

            using(var scope = _services.CreateScope())
            {
                _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await _mediator.Send(createOrderCommand);
            }

        }
    }
}
