using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Core.Domain.Models;
using BasketService.Api.IntegrationEvents.EventHandlers;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace BasketService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IIdentityService _identityService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<BasketController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BasketController(IBasketRepository basketRepository, IIdentityService identityService, IEventBus eventBus, ILogger<BasketController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _basketRepository = basketRepository;
            _identityService = identityService;
            _eventBus = eventBus;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Get()
        {

            //var user = _httpContextAccessor.HttpContext.User;

            //var x = 1;
            //var httpContext = _httpContextAccessor.HttpContext;
            //var test = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            //var header = AuthenticationHeaderValue.Parse(test);
            //var creds = header.Parameter;
            
            return Ok("Basket service is Up and Running...");
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPost]
        [Route("update")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket basket)
        {
            return Ok(await _basketRepository.UpdateBasketAsync(basket));
        }

        [HttpPost]
        [Route("additem")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> AddItemToBasket([FromBody] BasketItem basketItem)
        {

            var userId = _identityService.GetUserName();

            var basket = (await _basketRepository.GetBasketAsync(userId)) ?? new CustomerBasket(userId);

            basket.Items.Add(basketItem);
            await _basketRepository.UpdateBasketAsync(basket);

            return Ok();
        }

        [HttpPost]
        [Route("checkout")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CheckoutAsync([FromBody] BasketCheckout basketCheckout)
        {

            var userId = basketCheckout.Buyer;

            //basketCheckout.RequestId = Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty ? guid : basketCheckout.RequestId;

            var basket = await _basketRepository.GetBasketAsync(userId);

            if (basket == null) return BadRequest();

            var userName = _identityService.GetUserName();

            var eventMessage = new OrderCreatedIntegrationEvent(
                userId, userName, basketCheckout.City, basketCheckout.Street, basketCheckout.State, basketCheckout.Country,
                basketCheckout.ZipCode, basketCheckout.CardNumber, basketCheckout.CardHolderName, basketCheckout.CardExpiration,
                basketCheckout.CardSecurityNumber, basketCheckout.CardTypeId, basketCheckout.Buyer, basket);

            try
            {
                _eventBus.Publish(eventMessage);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"ERROR Publising integration event. EventId: {eventMessage.Id} from BasketService.App");
            }
            return Accepted();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task DeleteBasketAsync(string id)
        {
            await _basketRepository.DeleteBasketAsync(id);
        }

    }
}
