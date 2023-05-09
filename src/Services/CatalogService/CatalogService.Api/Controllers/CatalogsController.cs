using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogsController : ControllerBase
    {
        private readonly CatalogContext _context;
        private readonly CatalogSettings _settings;

        public CatalogsController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(_context));
            _settings = settings.Value;
        }

        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>),(int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>),(int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, [FromQuery] string? ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await GetItemsByIdsAsync(ids);

                if (!items.Any())
                {
                    return BadRequest("ids value invalid. Must be comma-separated list of numbers");
                }

                return Ok(items);
            }

            var totalItems = await _context.CatalogItems.LongCountAsync();
            var itemsOnPage = await _context.CatalogItems.OrderBy(c => c.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);
            var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);

        }

        private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int result), Value: result));

            if (!numIds.All(nId => nId.Ok)) return new List<CatalogItem>();

            var idsToSelect = numIds.Select(id => id.Value);

            var items = await _context.CatalogItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();
            items = ChangeUriPlaceholder(items);

            return items;
        }

        [HttpGet]
        [Route("items/{id:int}")]
        [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<CatalogItem>> ItemByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);
            var baseUri = _settings.PicBaseUrl;

            if(item != null)
            {
                item.PictureUrl = baseUri + item.PictureFileName;
                return Ok(item);
            }

            return NotFound();

        }

        [HttpGet]
        [Route("items/withname/{name:minlength(1)}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsWithNameAsync(string name,[FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {

            var itemsWithName = _context.CatalogItems
                            .Where(c => c.Name.StartsWith(name));

            var numberOfItems = await itemsWithName.LongCountAsync();

            var itemsOnPage = await itemsWithName
                            .Skip(pageSize * pageIndex)
                            .Take(pageSize)
                            .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            return Ok(new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, numberOfItems, itemsOnPage));

        }

        [HttpGet]
        [Route("items/type/{catalogTypeId}/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {

            var root = _context.CatalogItems
                        .Where(ci => ci.CatalogTypeId == catalogTypeId);
                        
            if(catalogBrandId.HasValue)
            {
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId); 
            }

            var totalItems = await root.LongCountAsync();
            var itemsOnPage = await root.Skip(pageSize * pageIndex)
                                        .Take(pageSize)
                                        .ToListAsync();

            itemsOnPage= ChangeUriPlaceholder(itemsOnPage);

            
            return Ok(new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));

        }

        [HttpGet]
        [Route("items/type/all/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByBrandIdAsync(int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {

            var root = (IQueryable<CatalogItem>)_context.CatalogItems;

            if (catalogBrandId.HasValue)
            {
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
            }

            var totalItems = await root.LongCountAsync();
            var itemsOnPage = await root.Skip(pageSize * pageIndex)
                                        .Take(pageSize)
                                        .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);


            return Ok(new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));

        }

        [HttpGet]
        [Route("catalogtypes")]
        [ProducesResponseType(typeof(List<CatalogType>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogType>>> CatalogTypesAsync()
        {

            return await _context.CatalogTypes.ToListAsync();
            
        }

        [HttpGet]
        [Route("catalogbrands")]
        [ProducesResponseType(typeof(List<CatalogBrand>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogBrand>>> CatalogBrandsAsync()
        {

            return await _context.CatalogBrands.ToListAsync();

        }

        [Route("items")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItem productToUpdate)
        {
            var catalogItem = await _context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == productToUpdate.Id);

            if (catalogItem == null) return NotFound(new { Message = $"Item with id {productToUpdate.Id} not found."});

            var oldPrice = catalogItem.Price;
            var raiseProductPriceChangeEvent = oldPrice != productToUpdate.Price;

            catalogItem = productToUpdate;
            _context.CatalogItems.Update(catalogItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ItemByIdAsync), new { id = productToUpdate.Id}, null);
        }

        [Route("items")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> CreateProductAsync([FromBody] CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price,
            };

            _context.CatalogItems.Update(item);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ItemByIdAsync), new { id = item.Id }, null);
        }

        [Route("{id}")]
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteProductAsync(int id)
        {
            var item = _context.CatalogItems.SingleOrDefault(ci => ci.Id == id);

            if (item == null) return NotFound();

            _context.Remove(item);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
        {
            var baseUri = _settings.PicBaseUrl;

            foreach (var item in items)
            {
                if(item != null)
                {
                    item.PictureUrl = baseUri + item.PictureFileName;
                }
            }

            return items;
        }
    }
}
