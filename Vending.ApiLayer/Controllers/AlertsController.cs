using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Vending.DataAccessLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly ILogger<AlertsController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AlertsController(ILogger<AlertsController> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Manuel olarak kritik durumları kontrol eder.
        /// </summary>
        /// <returns>Kritik stok ve bakiye durumları</returns>
        [HttpGet("check")]
        public async Task<IActionResult> CheckCriticalSituations()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<VendingContext>();

            var stockThreshold = 10; // Kritik stok eşiği
            var balanceThreshold = 5.0m; // Kritik bakiye eşiği

            var lowStockProducts = await context.Products
                .Where(p => p.Stock < stockThreshold)
                .ToListAsync();

            var lowBalanceUsers = await context.AppUsers
                .Where(u => u.CurrentBalance < balanceThreshold)
                .ToListAsync();

            if (!lowStockProducts.Any() && !lowBalanceUsers.Any())
            {
                return Ok("Kritik stok veya bakiye problemi yok.");
            }

            var alertBody = new StringBuilder();
            alertBody.AppendLine("🚨 Kritik Durum Bildirimi\n");

            if (lowStockProducts.Any())
            {
                alertBody.AppendLine("📦 Düşük Stoklu Ürünler:");
                foreach (var product in lowStockProducts)
                {
                    alertBody.AppendLine($"• {product.Name} - {product.Stock} adet");
                }
                alertBody.AppendLine();
            }

            if (lowBalanceUsers.Any())
            {
                alertBody.AppendLine("💰 Düşük Bakiyeli Kullanıcılar:");
                foreach (var user in lowBalanceUsers)
                {
                    alertBody.AppendLine($"• Kullanıcı Kodu: {user.UserCode} - Bakiye: {user.CurrentBalance:C}");
                }
                alertBody.AppendLine();
            }

            return Ok(alertBody.ToString());
        }

        /// <summary>
        /// Servisin durumunu kontrol eder.
        /// </summary>
        /// <returns>Servisin çalışıp çalışmadığı bilgisi</returns>
        [HttpGet("status")]
        public IActionResult GetServiceStatus()
        {
            // Bu örnekte, servisin durumunu kontrol etmek için basit bir yanıt döneriz.
            // Daha gelişmiş bir kontrol gerekiyorsa, servisin durumunu izleyen bir mekanizma eklenebilir.
            return Ok("CriticalAlertsService çalışıyor.");
        }
    }
}
