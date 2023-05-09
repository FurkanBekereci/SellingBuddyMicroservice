using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Domain.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;

namespace BasketService.Api.Infrastructure.Repository
{
    public class BasketRepository : IBasketRepository
    {
        private readonly ILogger<BasketRepository> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public BasketRepository(ILogger<BasketRepository> logger, ConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
            _database = _redis.GetDatabase() ;
        }

        public async Task<bool> DeleteBasketAsync(string id)
        {
            return await _database.KeyDeleteAsync(id);
        }

        public async Task<CustomerBasket> GetBasketAsync(string customerId)
        {
            var data = await _database.StringGetAsync(customerId);

            if (data.IsNullOrEmpty) return null;

            return JsonConvert.DeserializeObject<CustomerBasket>(data);
        }

        public IEnumerable<string> GetUsers()
        {
            var server = GetServer();
            var data = server.Keys();

            return data.Select(k => k.ToString());
        }

        private IServer GetServer()
        {
            var endPoint = _redis.GetEndPoints();
            return _redis.GetServer(endPoint.First());
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
        {            
            var created = await _database.StringSetAsync(basket.BuyerId, JsonConvert.SerializeObject(basket));

            if(!created)
            {
                _logger.LogInformation("Problem occured persisting the item.");
                return null;
            }

            _logger.LogInformation("Basket item persisted successfully.");

            return await GetBasketAsync(basket.BuyerId);
        }
    }
}
