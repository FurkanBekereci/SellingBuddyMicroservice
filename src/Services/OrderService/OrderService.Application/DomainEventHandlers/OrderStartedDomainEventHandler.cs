﻿using MediatR;
using OrderService.Application.Interfaces.Repositories;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderStartedDomainEventHandler : INotificationHandler<OrderStartedDomainEvent>
    {
        private readonly IBuyerRepository _buyerRepository;

        public OrderStartedDomainEventHandler(IBuyerRepository buyerRepository)
        {
            _buyerRepository = buyerRepository;
        }

        public async Task Handle(OrderStartedDomainEvent notification, CancellationToken cancellationToken)
        {
            var cardTypeId = notification.CardTypeId != 0 ? notification.CardTypeId : 1;
            var buyer = await _buyerRepository.GetSingleAsync(b => b.Name == notification.UserName, include => include.PaymentMethods);

            bool buyerOriginallyExisted = buyer != null;

            if (!buyerOriginallyExisted)
            {
                buyer = new Buyer(notification.UserName);
            }

            buyer.VerifyOrAddPaymentMethod(cardTypeId,
                                           $"Payment Method on {DateTime.UtcNow}",
                                           notification.CardNumber,
                                           notification.CardSecurityNumber,
                                           notification.CardHolderName,
                                           notification.CardExpiration,
                                           notification.Order.Id);

            var buyerUpdated = buyerOriginallyExisted
                ? _buyerRepository.Update(buyer)
                : await _buyerRepository.AddAsync(buyer);

            await _buyerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }



}
