using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.ApiGateway.Models.Baskets;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {

        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;

        public BasketController(ICatalogService catalogService, IBasketService basketService)
        {
            _catalogService = catalogService;
            _basketService = basketService;
        }

        [HttpPost]
        [Route("items")]
        protected async Task<ActionResult> AddBasketItemAsync([FromBody] AddBasketItemRequest request)
        {
            
            return Ok();
        }
    }
}
