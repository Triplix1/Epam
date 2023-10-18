using Business.Interfaces;
using Business.Models;
using Business.Services;
using Business.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductModel>>> Get([FromQuery] FilterSearchModel filterSearchModel)
        {
            if (filterSearchModel == null)
                return Ok(await _productService.GetAllAsync());

            return Ok(await _productService.GetByFilterAsync(filterSearchModel));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductModel>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ProductModel value)
        {
            try
            {
                await _productService.AddAsync(value);
            }
            catch (MarketException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(value);
        }

        // PUT: api/products/1
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int Id, [FromBody] ProductModel value)
        {
            if (Id != value.Id)
                throw new MarketException();

            await _productService.UpdateAsync(value);
            return Ok();
        }

        // DELETE: api/products/1
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok();
        }

        // GET: api/products/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<ProductCategoryModel>>> GetCategories()
        {
            return Ok(await _productService.GetAllProductCategoriesAsync());
        }

        [HttpPost("categories")]
        public async Task<ActionResult> PostCategory([FromBody] ProductCategoryModel value)
        {
            try
            {
                await _productService.AddCategoryAsync(value);
            }
            catch (MarketException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(value);
        }

        // PUT: api/products/categories1
        [HttpPut("categories/{id}")]
        public async Task<ActionResult> PutCategory(int Id, [FromBody] ProductCategoryModel value)
        {
            if (Id != value.Id)
                throw new MarketException();

            await _productService.UpdateCategoryAsync(value);
            return Ok();
        }

        // DELETE: api/products/categories/1
        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            await _productService.RemoveCategoryAsync(id);
            return Ok();
        }
    }
}
