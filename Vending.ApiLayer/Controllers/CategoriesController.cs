using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vending.BusinessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
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
    }
}
