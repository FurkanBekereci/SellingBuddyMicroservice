using WebApp.Application.Dtos;
using WebApp.Domain.Models.Orders;

namespace WebApp.Application.Services.Interfaces
{
    public interface IOrderService
    {
        BasketDTO MapOrderToBasket(Order order);
    }
}
