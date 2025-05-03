using Microsoft.AspNetCore.Mvc;
using Vending.BusinessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, IProductService productService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Category>> GetCategories()
        {
            _logger.LogInformation("Kategoriler getiriliyor.");
            var categories = _categoryService.TGetList();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public ActionResult<Category> GetCategory(int id)
        {
            _logger.LogInformation("Kategori getiriliyor: {id}", id);
            var category = _categoryService.TGetById(id);
            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {id}", id);
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost]
        public ActionResult<Category> PostCategory(Category category)
        {
            _logger.LogInformation("Yeni kategori ekleniyor.");
            _categoryService.TInsert(category);
            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, category);
        }

        [HttpPut("{id}")]
        public IActionResult PutCategory(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                _logger.LogWarning("Kategori ID uyuşmazlığı: {id}", id);
                return BadRequest();
            }

            _logger.LogInformation("Kategori güncelleniyor: {id}", id);
            _categoryService.TUpdate(category);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id)
        {
            _logger.LogInformation("Kategori siliniyor: {id}", id);
            var category = _categoryService.TGetById(id);
            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {id}", id);
                return NotFound();
            }

            _categoryService.TDelete(category);
            return NoContent();
        }

        [HttpGet("product-counts")]
        public ActionResult<Dictionary<string, int>> GetProductCountsByCategory()
        {
            _logger.LogInformation("Kategorilere göre ürün sayıları getiriliyor.");
            var productCounts = _categoryService.GetProductCountByCategory();
            return Ok(productCounts);
        }

        // ✅ New Endpoint: Get products by category ID
        [HttpGet("{categoryId}/products")]
        public ActionResult<IEnumerable<Product>> GetProductsByCategory(int categoryId)
        {
            _logger.LogInformation("Kategoriye göre ürünler getiriliyor: {categoryId}", categoryId);
            var category = _categoryService.TGetById(categoryId);

            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {categoryId}", categoryId);
                return NotFound(new { message = "Kategori bulunamadı." });
            }

            var products = _productService.GetProductsByCategory(categoryId);
            return Ok(products);
        }

        [HttpGet("{categoryId}/total-stock")]
        public ActionResult<int> GetTotalStockByCategory(int categoryId)
        {
            _logger.LogInformation("Kategoriye göre toplam stok getiriliyor: {categoryId}", categoryId);
            var category = _categoryService.TGetById(categoryId);

            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {categoryId}", categoryId);
                return NotFound(new { message = "Kategori bulunamadı." });
            }

            var totalStock = _categoryService.GetTotalStockByCategory(categoryId);
            return Ok(totalStock);
        }
        [HttpGet("{categoryId}/total-value")]
        public ActionResult<decimal> GetTotalValueByCategory(int categoryId)
        {
            _logger.LogInformation("Kategoriye göre toplam değer getiriliyor: {categoryId}", categoryId);
            var category = _categoryService.TGetById(categoryId);

            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {categoryId}", categoryId);
                return NotFound(new { message = "Kategori bulunamadı." });
            }

            var totalValue = _categoryService.GetTotalValueByCategory(categoryId);
            return Ok(totalValue);
        }
        [HttpGet("{categoryId}/average-price")]
        public ActionResult<decimal> GetAveragePriceByCategory(int categoryId)
        {
            _logger.LogInformation("Kategoriye göre ortalama fiyat getiriliyor: {categoryId}", categoryId);
            var category = _categoryService.TGetById(categoryId);

            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {categoryId}", categoryId);
                return NotFound(new { message = "Kategori bulunamadı." });
            }

            var averagePrice = _categoryService.GetAveragePriceByCategory(categoryId);
            return Ok(averagePrice);
        }
        [HttpGet("{categoryId}/most-expensive-product")]
        public ActionResult<Product> GetMostExpensiveProductByCategory(int categoryId)
        {
            _logger.LogInformation("Kategoriye göre en pahalı ürün getiriliyor: {categoryId}", categoryId);
            var category = _categoryService.TGetById(categoryId);

            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {categoryId}", categoryId);
                return NotFound(new { message = "Kategori bulunamadı." });
            }

            var product = _categoryService.GetMostExpensiveProductByCategory(categoryId);
            return Ok(product);
        }

        [HttpGet("{categoryId}/cheapest-product")]
        public ActionResult<Product> GetCheapestProductByCategory(int categoryId)
        {
            _logger.LogInformation("Kategoriye göre en ucuz ürün getiriliyor: {categoryId}", categoryId);
            var category = _categoryService.TGetById(categoryId);

            if (category == null)
            {
                _logger.LogWarning("Kategori bulunamadı: {categoryId}", categoryId);
                return NotFound(new { message = "Kategori bulunamadı." });
            }

            var product = _categoryService.GetCheapestProductByCategory(categoryId);
            return Ok(product);
        }
        [HttpGet("total-stock-across-all-categories")]
        public ActionResult<int> GetTotalStockAcrossAllCategories()
        {
            _logger.LogInformation("Tüm kategorilerdeki toplam stok getiriliyor.");
            var totalStock = _categoryService.GetTotalStockAcrossAllCategories();
            return Ok(totalStock);
        }

    }
}
