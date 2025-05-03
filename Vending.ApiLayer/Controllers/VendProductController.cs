using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vending.DataAccessLayer.Concrete;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendProductController : ControllerBase
    {
        private readonly VendingContext _context;

        public VendProductController(VendingContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Belirli bir otomatın ürünlerini listeler.
        /// </summary>
        /// <param name="vendId">Otomat ID</param>
        [HttpGet("{vendId}")]
        public async Task<IActionResult> GetProductsByVendId(int vendId)
        {
            var vendProducts = await _context.VendProducts
                .Include(vp => vp.Product)
                .Where(vp => vp.VendId == vendId)
                .Select(vp => new
                {
                    vp.ProductId,
                    vp.Product.Name,
                    vp.Product.Price,
                    vp.Stock
                })
                .ToListAsync();

            if (!vendProducts.Any())
            {
                return NotFound(new { Message = "Bu otomat için ürün bulunamadı." });
            }

            return Ok(vendProducts);
        }
    }
}