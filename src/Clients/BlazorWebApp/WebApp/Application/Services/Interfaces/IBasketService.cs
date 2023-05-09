using WebApp.Application.Dtos;
using WebApp.Domain.Models.Baskets;

namespace WebApp.Application.Services.Interfaces
{
    public interface IBasketService
    {

        Task<Basket> GetBasket();

        Task<Basket> UpdateBasket(Basket basket);

        Task AddItemToBasket(int productId);

        Task Checkout(BasketDTO basket);
    }
}
