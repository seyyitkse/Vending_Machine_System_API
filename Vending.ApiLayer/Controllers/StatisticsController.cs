using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vending.DataAccessLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly VendingContext _context;
        private readonly ILogger<OrderController> _logger;

        public StatisticsController(VendingContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Existing methods...

        [HttpGet("todaysRevenue")]
        public async Task<ActionResult<decimal>> GetTodaysRevenue()
        {
            _logger.LogInformation("Bugünkü satışların toplam geliri hesaplanıyor.");

            var today = DateTime.UtcNow.Date;
            var totalRevenue = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .SumAsync(o => o.TotalPrice);

            _logger.LogInformation("Bugünkü toplam gelir: {TotalRevenue}", totalRevenue);

            return Ok(totalRevenue);
        }

        [HttpGet("productCount")]
        public async Task<ActionResult<int>> GetProductCount()
        {
            _logger.LogInformation("Toplam ürün sayısı hesaplanıyor.");

            var productCount = await _context.Products.CountAsync();

            _logger.LogInformation("Toplam ürün sayısı: {ProductCount}", productCount);

            return Ok(productCount);
        }

        [HttpGet("vendCount")]
        public async Task<ActionResult<int>> GetVendCount()
        {
            _logger.LogInformation("Toplam otomat sayısı hesaplanıyor.");

            var vendCount = await _context.Vends.CountAsync();

            _logger.LogInformation("Toplam otomat sayısı: {VendCount}", vendCount);

            return Ok(vendCount);
        }

        [HttpGet("bestSellingProducts")]
        public async Task<ActionResult<IEnumerable<object>>> GetBestSellingProducts()
        {
            _logger.LogInformation("En çok satan ürünler getiriliyor.");

            var bestSellingProducts = await _context.Orders
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(o => o.Quantity),
                    ProductName = g.First().Product.Name
                })
                .OrderByDescending(p => p.TotalQuantity)
                .Take(10) // En çok satan ilk 10 ürünü al
                .ToListAsync();

            _logger.LogInformation("En çok satan ürünler getirildi.");

            return Ok(bestSellingProducts);
        }

        [HttpGet("userCount")]
        public async Task<ActionResult<int>> GetUserCount()
        {
            _logger.LogInformation("Toplam kullanıcı sayısı hesaplanıyor.");

            var userCount = await _context.Users.CountAsync();

            _logger.LogInformation("Toplam kullanıcı sayısı: {UserCount}", userCount);

            return Ok(userCount);
        }

        [HttpGet("topSellingVends")]
        public async Task<ActionResult<IEnumerable<object>>> GetTopSellingVends()
        {
            _logger.LogInformation("En çok satış yapan otomatlar getiriliyor.");

            var topSellingVends = await _context.Orders
                .GroupBy(o => o.VendId)
                .Select(g => new
                {
                    VendId = g.Key,
                    TotalSales = g.Sum(o => o.TotalPrice),
                    VendName = g.First().Vend.Name
                })
                .OrderByDescending(v => v.TotalSales)
                .Take(10) // En çok satış yapan ilk 10 otomatı al
                .ToListAsync();

            _logger.LogInformation("En çok satış yapan otomatlar getirildi.");

            return Ok(topSellingVends);
        }

        [HttpGet("monthlyRevenue")]
        public async Task<ActionResult<IEnumerable<object>>> GetMonthlyRevenue()
        {
            _logger.LogInformation("Aylık gelir hesaplanıyor.");

            var monthlyRevenue = await _context.Orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
                .ToListAsync();

            _logger.LogInformation("Aylık gelir hesaplandı.");

            return Ok(monthlyRevenue);
        }

        [HttpGet("criticalStockProducts")]
        public async Task<ActionResult<IEnumerable<object>>> GetCriticalStockProducts()
        {
            _logger.LogInformation("Stok durumu kritik olan ürünler getiriliyor.");

            var criticalStockProducts = await _context.Products
                .Where(p => p.IsCriticalStock)
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Stock
                })
                .ToListAsync();

            _logger.LogInformation("Stok durumu kritik olan ürünler getirildi.");

            return Ok(criticalStockProducts);
        }

        [HttpGet("recentlyAddedProducts")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecentlyAddedProducts()
        {
            _logger.LogInformation("Son eklenen ürünler getiriliyor.");

            var recentlyAddedProducts = await _context.Products
                .OrderByDescending(p => p.ProductId)
                .Take(10) // Son eklenen ilk 10 ürünü al
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    p.Stock
                })
                .ToListAsync();

            _logger.LogInformation("Son eklenen ürünler getirildi.");

            return Ok(recentlyAddedProducts);
        }
    }
}