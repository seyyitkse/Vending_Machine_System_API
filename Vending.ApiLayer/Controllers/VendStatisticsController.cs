using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vending.DataAccessLayer.Concrete;
using System.Globalization;

namespace Vending.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendStatisticsController : ControllerBase
    {
        private readonly VendingContext _context;

        public VendStatisticsController(VendingContext context)
        {
            _context = context;
        }

        // 1. Monthly Sales per Vend
        [HttpGet("monthly-sales/{vendId}")]
        public async Task<IActionResult> GetMonthlySales(
            int vendId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var now = DateTime.Now;
            var start = startDate ?? new DateTime(now.Year, 1, 1);
            var end = endDate ?? now;

            var sales = await _context.Orders
                .Where(o => o.VendId == vendId && o.OrderDate >= start && o.OrderDate <= end)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    MonthNumber = g.Key,
                    TotalSales = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.MonthNumber)
                .ToListAsync();

            var result = sales.Select(x => new
            {
                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.MonthNumber),
                x.TotalSales
            });

            return Ok(result);
        }

        // 2. Product Distribution per Vend
        [HttpGet("product-distribution/{vendId}")]
        public async Task<IActionResult> GetProductDistribution(
            int vendId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.Now;

            var distribution = await _context.Orders
                .Where(o => o.VendId == vendId && o.OrderDate >= start && o.OrderDate <= end)
                .Select(o => new { o.Product.Name, o.Quantity })
                .ToListAsync();

            var grouped = distribution
                .GroupBy(x => x.Name)
                .Select(g => new
                {
                    Product = g.Key,
                    Count = g.Sum(x => x.Quantity)
                })
                .ToList();

            return Ok(grouped);
        }

        // 3. Daily Visitor Count per Vend (Order count per day)
        [HttpGet("daily-visitors/{vendId}")]
        public async Task<IActionResult> GetDailyVisitors(
            int vendId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var visitors = await _context.Orders
                .Where(o => o.VendId == vendId && o.OrderDate >= start && o.OrderDate <= end)
                .Select(o => o.OrderDate.Date)
                .ToListAsync();

            var grouped = visitors
                .GroupBy(date => date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .ToList();

            return Ok(grouped);
        }

        // 4. Regional Performance (Sales by Vend Location)
        [HttpGet("regional-performance")]
        public async Task<IActionResult> GetRegionalPerformance(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.Now;

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .Include(o => o.Vend)
                .Select(o => new { VendName = o.Vend.Name, o.TotalPrice })
                .ToListAsync();

            var grouped = orders
                .GroupBy(x => x.VendName)
                .Select(g => new
                {
                    Vend = g.Key,
                    TotalSales = g.Sum(x => x.TotalPrice)
                })
                .ToList();

            return Ok(grouped);
        }

        // 5. Category Distribution per Vend
        [HttpGet("category-distribution/{vendId}")]
        public async Task<IActionResult> GetCategoryDistribution(
            int vendId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.Now;

            var orders = await _context.Orders
                .Where(o => o.VendId == vendId && o.OrderDate >= start && o.OrderDate <= end)
                .Include(o => o.Product)
                .ThenInclude(p => p.Category)
                .Select(o => new { CategoryName = o.Product.Category.Name, o.Quantity })
                .ToListAsync();

            var grouped = orders
                .GroupBy(x => x.CategoryName)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Sum(x => x.Quantity)
                })
                .ToList();

            return Ok(grouped);
        }

        [HttpGet("last-orders/{vendId}")]
        public async Task<IActionResult> GetLastOrdersByVend(
            int vendId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.Now;

            var lastOrders = await _context.Orders
                .Where(o => o.VendId == vendId && o.OrderDate >= start && o.OrderDate <= end)
                .Include(o => o.Product)
                .Include(o => o.AppUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(20)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.TotalPrice,
                    o.Quantity,
                    ProductName = o.Product.Name,
                    UserName = o.AppUser.FullName ?? o.AppUser.UserName,
                    UserEmail = o.AppUser.Email
                })
                .ToListAsync();

            return Ok(lastOrders);
        }
    }
}