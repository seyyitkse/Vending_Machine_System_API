using Microsoft.AspNetCore.Mvc;
using Vending.BusinessLayer.Abstract;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendController : ControllerBase
    {
        private readonly IVendService _vendService;
        private readonly ILogger<VendController> _logger;

        public VendController(IVendService vendService, ILogger<VendController> logger)
        {
            _vendService = vendService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Tüm otomatlar getiriliyor");
            var vends = _vendService.TGetList();
            return Ok(vends);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation($"ID'si {id} olan otomat getiriliyor");
            var vend = _vendService.TGetById(id);
            if (vend == null)
            {
                _logger.LogWarning($"ID'si {id} olan otomat bulunamadı");
                return NotFound();
            }
            return Ok(vend);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Vend vend)
        {
            if (vend == null)
            {
                _logger.LogWarning("Otomat nesnesi null");
                return BadRequest();
            }

            _logger.LogInformation("Yeni bir otomat oluşturuluyor");
            _vendService.TInsert(vend);
            return CreatedAtAction(nameof(GetById), new { id = vend.Id }, vend);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Vend vend)
        {
            if (vend == null || vend.Id != id)
            {
                _logger.LogWarning("Otomat nesnesi null veya ID uyuşmazlığı");
                return BadRequest();
            }

            _logger.LogInformation($"ID'si {id} olan otomat güncelleniyor");
            var existingVend = _vendService.TGetById(id);
            if (existingVend == null)
            {
                _logger.LogWarning($"ID'si {id} olan otomat bulunamadı");
                return NotFound();
            }

            _vendService.TUpdate(vend);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation($"ID'si {id} olan otomat siliniyor");
            var vend = _vendService.TGetById(id);
            if (vend == null)
            {
                _logger.LogWarning($"ID'si {id} olan otomat bulunamadı");
                return NotFound();
            }

            _vendService.TDelete(vend);
            return NoContent();
        }
    }
}
