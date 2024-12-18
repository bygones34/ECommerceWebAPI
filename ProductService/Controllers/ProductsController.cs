using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _productDbContext;

        public ProductsController(ProductDbContext dbContext)
        {
            _productDbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsAsync()
        {
            var products = await _productDbContext.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            var product = await _productDbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductAsync([FromBody] Product product)
        {
            _productDbContext.Products.Add(product);
            await _productDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductByIdAsync(int id, [FromBody] Product updatedProduct)
        {
            var product = await _productDbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;

            await _productDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductByıdAsync(int id)
        {
            var product = await _productDbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _productDbContext.Products.Remove(product);
            await _productDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}