using WebApp.Application.Dtos;
using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models.Orders;

namespace WebApp.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        public BasketDTO MapOrderToBasket(Order order)
        {
            order.CardExpirationApiFormat();

            return new BasketDTO
            {
                City = order.City,
                Country = order.Country,
                Street = order.Street,
                State = order.State,
                ZipCode = order.ZipCode,
                CardNumber = order.CardNumber,
                CardHolderName = order.CardHolderName,
                CardExpiration = order.CardExpiration,
                CardSecurityNumber = order.CardSecurityNumber,
                CardTypeId = order.CardTypeId == 0 ? 1 : order.CardTypeId,
                Buyer = order.Buyer
            };
        }
    }
}
