using Web.ApiGateway.Extensions;
using Web.ApiGateway.Models.Baskets;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.Services.Implementations
{
    public class BasketService : IBasketService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BasketService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BasketData> GetById(string id)
        {
            using (var client = _httpClientFactory.CreateClient("basket"))
            {
                var response = await client.GetAsync<BasketData>(id);

                return response ?? new BasketData(id);
            }
        }

        public async Task<BasketData> UpdateAsync(BasketData basketData)
        {
            using (var client = _httpClientFactory.CreateClient("basket"))
            {
                return await client.PostWithGettingResponseAsync<BasketData, BasketData>($"update", basketData);

            }
        }
    }
}
