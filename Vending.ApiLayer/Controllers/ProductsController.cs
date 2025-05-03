using Microsoft.AspNetCore.Mvc;
using Vending.BusinessLayer.Abstract;
using Vending.DtoLayer.Dtos.ProductDto;
using Vending.EntityLayer.Concrete;

namespace Vending.API.Controllers
{
    /// <summary>
    /// ProductsController handles CRUD operations for products.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        /// <summary>
        /// Constructor for ProductsController.
        /// </summary>
        /// <param name="productService">Service for product operations.</param>
        /// <param name="logger">Logger for logging operations.</param>
        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Urunleri listeleme islemi yapilmaktadir.
        /// </summary>
        /// <returns>Urunleri listeleme islemi yapilmaktadir.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ResultProductDto>> GetProducts()
        {
            _logger.LogInformation("Fetching all products with related data.");
            var products = _productService.GetListByRelations().Select(p => new ResultProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                IsCriticalStock = p.IsCriticalStock,
                CategoryName = p.Category?.Name ?? "Unknown",
                BrandName = p.Brand?.Name ?? "Unknown"
            }).ToList();

            _logger.LogInformation("Successfully fetched {Count} products.", products.Count);
            return Ok(products);
        }

        /// <summary>
        /// ID'ye gore urun getirme islemi yapilmaktadir.
        /// </summary>
        /// <param name="id">ID'ye gore urun getirme islemi yapilmaktadir.</param>
        /// <returns>Product details as ProductDto.</returns>
        [HttpGet("{id}")]
        public ActionResult<ResultProductDto> GetProduct(int id)
        {
            _logger.LogInformation("Fetching product with ID: {Id} and related data.", id);
            var product = _productService.GetListByRelationsWithID(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID: {Id} not found.", id);
                return NotFound();
            }

            var productDto = new ResultProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                IsCriticalStock = product.IsCriticalStock,
                CategoryName = product.Category?.Name ?? "Unknown",
                BrandName = product.Brand?.Name ?? "Unknown"
            };

            _logger.LogInformation("Successfully fetched product with ID: {Id}", id);
            return Ok(productDto);
        }

        /// <summary>
        /// Urun olusturma islemi icin kullanilmaktadir.
        /// </summary>
        /// <param name="createProductDto">Urun olusturma islemi icin kullanilmaktadir.</param>
        /// <returns>Created product details.</returns>
        [HttpPost]
        public ActionResult<ResultProductDto> PostProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product data provided for creation.");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating a new product.");
            var product = new Product
            {
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Stock = createProductDto.Stock,
                CategoryId = createProductDto.CategoryId,
                BrandId = createProductDto.BrandId
            };

            _productService.TInsert(product);

            var createdProduct = _productService.GetListByRelationsWithID(product.ProductId);

            var productDto = new ResultProductDto
            {
                ProductId = createdProduct.ProductId,
                Name = createdProduct.Name,
                Price = createdProduct.Price,
                Stock = createdProduct.Stock,
                IsCriticalStock = createdProduct.IsCriticalStock,
                CategoryName = createdProduct.Category?.Name ?? "Unknown",
                BrandName = createdProduct.Brand?.Name ?? "Unknown"
            };

            _logger.LogInformation("Product created successfully with ID: {Id}", product.ProductId);
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, productDto);
        }

        /// <summary>
        /// Urun guncelleme islemi icin kullanilmaktadir.
        /// </summary>
        /// <param name="id">Product ID to update.</param>
        /// <param name="updateProductDto">Urun guncelleme islemi icin kullanilmaktadir.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")]
        public IActionResult PutProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (id != updateProductDto.ProductId)
            {
                _logger.LogWarning("Product ID mismatch: {Id}", id);
                return BadRequest();
            }

            var product = _productService.GetListByRelationsWithID(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID: {Id} not found for update.", id);
                return NotFound();
            }

            _logger.LogInformation("Updating product with ID: {Id}", id);
            product.Name = updateProductDto.Name;
            product.Price = updateProductDto.Price;
            product.Stock = updateProductDto.Stock;
            product.CategoryId = updateProductDto.CategoryId;
            product.BrandId = updateProductDto.BrandId;
            product.IsCriticalStock = updateProductDto.IsCriticalStock;

            _productService.TUpdate(product);
            _logger.LogInformation("Product with ID: {Id} updated successfully.", id);
            return NoContent();
        }

        /// <summary>
        /// ID'ye gore silme islemi yapilmaktadir.
        /// </summary>
        /// <param name="id">ID'ye gore silme islemi yapilmaktadir.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with ID: {Id}", id);
            var product = _productService.GetListByRelationsWithID(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID: {Id} not found for deletion.", id);
                return NotFound();
            }

            _productService.TDelete(product);
            _logger.LogInformation("Product with ID: {Id} deleted successfully.", id);
            return NoContent();
        }

        /// <summary>
        /// Kritik stok seviyesindeki urunleri listelemektedir.
        /// </summary>
        /// <returns>Kritik stok seviyesindeki urunleri listelemektedir.</returns>
        [HttpGet("critical-stock-products")]
        public ActionResult<IEnumerable<ResultProductDto>> GetCriticalStockProducts()
        {
            _logger.LogInformation("Fetching products with critical stock levels.");
            var products = _productService.GetCriticalStockProducts().Select(p => new ResultProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                IsCriticalStock = p.IsCriticalStock,
                CategoryName = p.Category?.Name ?? "Unknown",
                BrandName = p.Brand?.Name ?? "Unknown"
            }).ToList();

            _logger.LogInformation("Successfully fetched {Count} products with critical stock levels.", products.Count);
            return Ok(products);
        }
    }
}
