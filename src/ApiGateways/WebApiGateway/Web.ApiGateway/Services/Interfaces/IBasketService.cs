using Web.ApiGateway.Models.Baskets;

namespace Web.ApiGateway.Services.Interfaces
{
    public interface IBasketService
    {

        Task<BasketData> GetById(string id);

        Task<BasketData> UpdateAsync(BasketData basketData);

    }
}
