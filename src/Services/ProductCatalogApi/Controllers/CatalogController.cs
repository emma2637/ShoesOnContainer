using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalogApi.Data;
using ProductCatalogApi.Domain;
using ProductCatalogApi.ViewModels;

namespace ProductCatalogApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        //this has been injected in the startup class
        //to read updated data. Is useful in scenarios where options should be recomputed on every request 

        private readonly IOptionsSnapshot<CatalogSettings> _settings;
        public CatalogController(CatalogContext catalogContext, IOptionsSnapshot<CatalogSettings> snapshot)
        {
            _catalogContext = catalogContext;
            _settings = snapshot;

            //disable tracking behavior
            // will make the app faster
            ((DbContext)catalogContext).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [HttpGet]
        [Route("[action]")] // to find catalogtpes method
        public async Task<IActionResult> CatalogTypes()
        {
            var items = await _catalogContext.CatalogTypes.ToListAsync();

            return Ok(items);

        }
        [HttpGet]
        [Route("[action]")] // to find catalogtpes method
        public async Task<IActionResult> CatalogBrands()
        {
            var items = await _catalogContext.CatalogBrands.ToListAsync();

            return Ok(items);

        }

        [HttpGet]
        [Route("items/{id:int}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);
            if (item != null)
            {
                item.PictureUrl = item.PictureUrl.Replace("http://externalcatalogbaseurltobereplaced", _settings.Value.ExternalCatalogBaseUrl);
                return Ok(item);
            }
            return NotFound();
        }
        //Get api/Catalog/items[?pageSize = 4&pageIndex=3]
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Items([FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {

            var totalItems = await _catalogContext.CatalogItems.LongCountAsync();
            //list of item we are going to send to the page
            var itemsOnPage = await _catalogContext.CatalogItems.OrderBy(x => x.Name).Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            //replace current external url to local
            itemsOnPage = ChangeUrlPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        //Get api/Catalog/items/withname/Wonder?pageSize = 4&pageIndex=3
        [HttpGet]
        [Route("[action]/withname/{name:minlength(1)}")]
        public async Task<IActionResult> Items(string name, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {

            var totalItems = await _catalogContext.CatalogItems.Where(x => x.Name.StartsWith(name)).LongCountAsync();
            //list of item we are going to send to the page
            var itemsOnPage = await _catalogContext.CatalogItems.
                Where(x => x.Name.StartsWith(name)).OrderBy(x => x.Name)
                .Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            //replace current external url to local
            itemsOnPage = ChangeUrlPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        //Get api/Catalog/items/type/1/brand/null[?pageSize = 4 & pageIndex=3]
        [HttpGet]
        [Route("[action]/type/{catalogTypeId}/brand/{catalogBrandId}")]
        public async Task<IActionResult> Items(int? catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems; // not sent the call to db

            if (catalogTypeId.HasValue)
            {
                root = root.Where(x => x.CatalogTypeId == catalogTypeId);
            }
            if (catalogBrandId.HasValue)
            {
                root = root.Where(x => x.CatalogBrandId == catalogBrandId);
            }


            var totalItems = await root.LongCountAsync();
            //list of item we are going to send to the page
            var itemsOnPage = await root
                .OrderBy(x => x.Name)
                .Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            //replace current external url to local
            itemsOnPage = ChangeUrlPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        //Post api/catalog/items
        [HttpPost]
        [Route("items")]
        public async Task<IActionResult> CreateProduct([FromBody] CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                //CatalogBrand = product.CatalogBrand,
                //CatalogType = product.CatalogType,
                Price = product.Price,
                //PictureUrl = product.PictureUrl
            };
            _catalogContext.CatalogItems.Add(item);
            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = item.Id });

        }

        [HttpPut]
        [Route("items")]
        public async Task<IActionResult> UpdateProduct([FromBody] CatalogItem productToUpdate)
        {
            var catalogItem = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == productToUpdate.Id);

            if (catalogItem == null)
            {
                return NotFound(new { message = $"Item with id {productToUpdate.Id} not found." });
            }
            catalogItem = productToUpdate;
            _catalogContext.CatalogItems.Update(catalogItem);
            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = productToUpdate.Id });

        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteProduct([FromBody] CatalogItem productToRemove)
        {
            var catalogItem = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == productToRemove.Id);

            if (catalogItem == null)
            {
                return NotFound(new { message = $"Item with id {productToRemove.Id} not found." });
            }

            _catalogContext.CatalogItems.Remove(productToRemove);
            await _catalogContext.SaveChangesAsync();

            return NoContent();
        }


        private List<CatalogItem> ChangeUrlPlaceholder(List<CatalogItem> catalogItems)
        {
            catalogItems.ForEach(x => x.PictureFileName = x.PictureFileName.Replace("http://externalcatalogbaseurltobereplaced", _settings.Value.ExternalCatalogBaseUrl));

            return catalogItems;
        }
    }
}
