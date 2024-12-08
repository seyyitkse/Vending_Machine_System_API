using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vending.BusinessLayer.Abstract;
using Vending.BusinessLayer.Concrete;
using Vending.EntityLayer.Concrete;

namespace Vending.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            _logger.LogInformation("Ürünler getiriliyor.");
            var products = _productService.TGetList();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            _logger.LogInformation("Ürün getiriliyor: {id}", id);
            var product = _productService.TGetById(id);
            if (product == null)
            {
                _logger.LogWarning("Ürün bulunamadı: {id}", id);
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public ActionResult<Product> PostProduct(Product product)
        {
            _logger.LogInformation("Yeni ürün ekleniyor.");
            _productService.TInsert(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
        }

        [HttpPut("{id}")]
        public IActionResult PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                _logger.LogWarning("Ürün ID uyuşmazlığı: {id}", id);
                return BadRequest();
            }

            _logger.LogInformation("Ürün güncelleniyor: {id}", id);
            _productService.TUpdate(product);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            _logger.LogInformation("Ürün siliniyor: {id}", id);
            var product = _productService.TGetById(id);
            if (product == null)
            {
                _logger.LogWarning("Ürün bulunamadı: {id}", id);
                return NotFound();
            }

            _productService.TDelete(product);
            return NoContent();
        }
    }
}