using Microsoft.AspNetCore.Mvc;
using Vending.BusinessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IBrandService brandService, ILogger<BrandController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Tüm markalar getiriliyor");
            var brands = _brandService.TGetList();
            return Ok(brands);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation($"ID'si {id} olan marka getiriliyor");
            var brand = _brandService.TGetById(id);
            if (brand == null)
            {
                _logger.LogWarning($"ID'si {id} olan marka bulunamadı");
                return NotFound();
            }
            return Ok(brand);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Brand brand)
        {
            if (brand == null)
            {
                _logger.LogWarning("Marka nesnesi null");
                return BadRequest();
            }

            _logger.LogInformation("Yeni bir marka oluşturuluyor");
            _brandService.TInsert(brand);
            return CreatedAtAction(nameof(GetById), new { id = brand.BrandId }, brand);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Brand brand)
        {
            if (brand == null || brand.BrandId != id)
            {
                _logger.LogWarning("Marka nesnesi null veya ID uyuşmazlığı");
                return BadRequest();
            }

            _logger.LogInformation($"ID'si {id} olan marka güncelleniyor");
            var existingBrand = _brandService.TGetById(id);
            if (existingBrand == null)
            {
                _logger.LogWarning($"ID'si {id} olan marka bulunamadı");
                return NotFound();
            }

            _brandService.TUpdate(brand);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation($"ID'si {id} olan marka siliniyor");
            var brand = _brandService.TGetById(id);
            if (brand == null)
            {
                _logger.LogWarning($"ID'si {id} olan marka bulunamadı");
                return NotFound();
            }

            _brandService.TDelete(brand);
            return NoContent();
        }
    }
}

