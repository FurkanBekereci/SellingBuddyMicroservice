using Web.ApiGateway.Extensions;
using Web.ApiGateway.Models.Catalogs;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.Services.Implementations
{
    public class CatalogService : ICatalogService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CatalogService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<CatalogItem> GetCatalogItemAsync(int id)
        {
            using (var client = _httpClientFactory.CreateClient("catalog"))
            {
                var response = await client.GetAsync<CatalogItem>($"items/{id}");

                return response;
            }
        }

        public Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync(IEnumerable<int> ids)
        {
            return null;
        }
    }
}
