using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models;
using WebApp.Domain.Models.Catalog;
using WebApp.Extensions;

namespace WebApp.Application.Services.Implementations
{
    public class CatalogService : ICatalogService
    {

        private readonly HttpClient _httpClient;

        public CatalogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedItemsViewModel<CatalogItem>> GetCatalogItems()
        {
            return await _httpClient.GetAsync<PaginatedItemsViewModel<CatalogItem>>("/catalogs/items");
        }
    }
}
