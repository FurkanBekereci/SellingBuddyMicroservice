using WebApp.Application.Dtos;
using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models.Baskets;
using WebApp.Extensions;

namespace WebApp.Application.Services.Implementations
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _apiClient;
        private readonly IIdentityService _identityService;
        private readonly ILogger<BasketService> _logger;

        public BasketService(HttpClient apiClient, IIdentityService identityService, ILogger<BasketService> logger)
        {
            _apiClient = apiClient;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task AddItemToBasket(int productId)
        {
            var model = new
            {
                CatalogItemId = productId,
                Quantity = 1,
                BasketId = _identityService.GetUserName()

            };

            await _apiClient.PostOnlyAsync("basket/items", model);
        }

        public Task Checkout(BasketDTO basket)
        {
            return _apiClient.PostOnlyAsync("basket/checkout", basket);
        }

        public async Task<Basket> GetBasket()
        {
            //if (!_identityService.IsLoggedIn) return null;

            var userName = _identityService.GetUserName();
            var basket = await _apiClient.GetAsync<Basket>($"basket/{userName}");

            return basket ?? new Basket() { BuyerId = userName };

        }

        public async Task<Basket> UpdateBasket(Basket basket)
        {

            var updatedBasket = await _apiClient.PostWithGettingResponseAsync<Basket, Basket>("basket/update", basket);

            return updatedBasket;

        }


    }
}
