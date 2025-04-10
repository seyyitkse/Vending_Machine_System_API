//burası endpoitnle calisan send-missing-invoices


using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class InvoiceTimerService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly ILogger<InvoiceTimerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public InvoiceTimerService(ILogger<InvoiceTimerService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InvoiceTimerService başlatıldı.");

        // 10 dakikada bir çalışacak şekilde ayarlıyoruz (ilk 5 saniye sonra başlasın)
        _timer = new Timer(SendInvoiceRequest, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    private async void SendInvoiceRequest(object state)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("https://localhost:44395/api/Order/send-oldest-unpaid-invoice", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Fatura gönderim endpoint’i başarıyla çağrıldı.");
            }
            else
            {
                _logger.LogWarning("Fatura endpoint çağrısı başarısız: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatura endpoint’ine istek atılırken hata oluştu.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InvoiceTimerService durduruluyor...");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

//burası metotla calisan
//using Microsoft.EntityFrameworkCore;
//using Vending.ApiLayer.Services.Abstract;
//using Vending.DataAccessLayer.Concrete;

//public class InvoiceTimerService : IHostedService, IDisposable
//{
//    private readonly ILogger<InvoiceTimerService> _logger;
//    private readonly IServiceProvider _serviceProvider;
//    private Timer _timer;

//    public InvoiceTimerService(ILogger<InvoiceTimerService> logger, IServiceProvider serviceProvider)
//    {
//        _logger = logger;
//        _serviceProvider = serviceProvider;
//    }

//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("InvoiceTimerService başlatıldı.");

//        // Timer’ı her dakika bir çalışacak şekilde ayarlıyoruz
//        _timer = new Timer(SendInvoiceRequest, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
//        return Task.CompletedTask;
//    }

//    private async void SendInvoiceRequest(object state)
//    {
//        try
//        {
//            // Scoped hizmeti IServiceProvider kullanarak alıyoruz
//            using (var scope = _serviceProvider.CreateScope())
//            {
//                var backgroundTaskService = scope.ServiceProvider.GetRequiredService<IBackgroundTaskService>();
//                var context = scope.ServiceProvider.GetRequiredService<VendingContext>();

//                // Fatura gönderim işlemi
//                var pendingOrder = await context.Orders
//                    .Include(o => o.Product)
//                    .Include(o => o.Vend)
//                    .Include(o => o.AppUser)
//                    .Where(o => !o.IsInvoiceSent)
//                    .OrderBy(o => o.OrderDate)
//                    .FirstOrDefaultAsync();

//                if (pendingOrder == null)
//                {
//                    _logger.LogInformation("Gönderilmemiş fatura bulunamadı.");
//                    return;
//                }

//                var invoiceModel = new InvoiceViewModel
//                {
//                    OrderId = pendingOrder.OrderId,
//                    UserName = pendingOrder.AppUser.UserName,
//                    Email = pendingOrder.AppUser.Email,
//                    ProductName = pendingOrder.Product.Name,
//                    Quantity = pendingOrder.Quantity,
//                    Price = pendingOrder.Product.Price,
//                    TotalPrice = pendingOrder.TotalPrice,
//                    OrderDate = pendingOrder.OrderDate,
//                    VendName = pendingOrder.Vend?.Name ?? "Bilinmiyor"
//                };

//                await backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);

//                // Fatura gönderildiğinde, durumu güncelle
//                pendingOrder.IsInvoiceSent = true;
//                context.Orders.Update(pendingOrder);
//                await context.SaveChangesAsync();

//                _logger.LogInformation("Fatura gönderildi ve işaretlendi: {OrderId}", pendingOrder.OrderId);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Fatura gönderimi sırasında hata oluştu.");
//        }
//    }

//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("InvoiceTimerService durduruluyor...");
//        _timer?.Change(Timeout.Infinite, 0);
//        return Task.CompletedTask;
//    }

//    public void Dispose()
//    {
//        _timer?.Dispose();
//    }
//}




//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Vending.DataAccessLayer.Concrete;
//using Vending.ApiLayer.Services.Abstract;

//public class InvoiceTimerService : BackgroundService
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly ILogger<InvoiceTimerService> _logger;

//    public InvoiceTimerService(IServiceProvider serviceProvider, ILogger<InvoiceTimerService> logger)
//    {
//        _serviceProvider = serviceProvider;
//        _logger = logger;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        _logger.LogInformation("InvoiceTimerService başlatıldı.");

//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                using (var scope = _serviceProvider.CreateScope())
//                {
//                    var context = scope.ServiceProvider.GetRequiredService<VendingContext>();
//                    var backgroundTaskService = scope.ServiceProvider.GetRequiredService<IBackgroundTaskService>();

//                    // Gönderilmemiş ilk siparişi al
//                    var pendingOrder = await context.Orders
//                        .Include(o => o.Product)
//                        .Include(o => o.Vend)
//                        .Include(o => o.AppUser)
//                        .Where(o => !o.IsInvoiceSent)
//                        .OrderBy(o => o.OrderDate)
//                        .FirstOrDefaultAsync(stoppingToken);

//                    if (pendingOrder != null)
//                    {
//                        _logger.LogInformation("Gönderilmemiş fatura bulundu: {OrderId}", pendingOrder.OrderId);

//                        var invoiceModel = new InvoiceViewModel
//                        {
//                            OrderId = pendingOrder.OrderId,
//                            UserName = pendingOrder.AppUser.UserName,
//                            Email = pendingOrder.AppUser.Email,
//                            ProductName = pendingOrder.Product.Name,
//                            Quantity = pendingOrder.Quantity,
//                            Price = pendingOrder.Product.Price,
//                            TotalPrice = pendingOrder.TotalPrice,
//                            OrderDate = pendingOrder.OrderDate,
//                            VendName = pendingOrder.Vend?.Name ?? "Bilinmiyor"
//                        };

//                        try
//                        {
//                            await backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);

//                            // Fatura gönderildikten sonra işaretle
//                            pendingOrder.IsInvoiceSent = true;
//                            context.Orders.Update(pendingOrder);
//                            await context.SaveChangesAsync(stoppingToken);

//                            _logger.LogInformation("Fatura başarıyla gönderildi: {OrderId}", pendingOrder.OrderId);
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogError(ex, "Fatura gönderimi sırasında hata oluştu: {OrderId}", pendingOrder.OrderId);
//                        }
//                    }
//                    else
//                    {
//                        _logger.LogInformation("Gönderilecek fatura bulunamadı.");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "InvoiceTimerService çalışırken bir hata oluştu.");
//            }

//            // Belirli bir süre bekle (örneğin 1 dakika)
//            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//        }
//    }
//}
