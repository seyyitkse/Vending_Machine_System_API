using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Vending.DataAccessLayer.Concrete;
using Vending.EntityLayer.Concrete;
using Vending.ApiLayer.Services.Abstract;

public class InvoiceSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InvoiceSchedulerService> _logger;

    public InvoiceSchedulerService(IServiceProvider serviceProvider, ILogger<InvoiceSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Fatura gönderimi kontrol ediliyor...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VendingContext>(); // EF DbContext
                var backgroundTaskService = scope.ServiceProvider.GetRequiredService<IBackgroundTaskService>();

                var pendingOrders = await context.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Vend)
                    .Include(o => o.AppUser)
                    .Where(o => !o.IsInvoiceSent)
                    .ToListAsync(stoppingToken);

                foreach (var order in pendingOrders)
                {
                    try
                    {
                        var invoiceModel = new InvoiceViewModel
                        {
                            OrderId = order.OrderId,
                            UserName = order.AppUser.UserName,
                            Email = order.AppUser.Email,
                            ProductName = order.Product.Name,
                            Quantity = order.Quantity,
                            Price = order.Product.Price,
                            TotalPrice = order.TotalPrice,
                            OrderDate = order.OrderDate,
                            VendName = order.Vend?.Name ?? "Bilinmiyor"
                        };

                        await backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);

                        // Başarılıysa işaretle
                        order.IsInvoiceSent = true;
                        context.Orders.Update(order);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Siparişin faturası gönderilemedi: {OrderId}", order.OrderId);
                    }
                }
            }

            // 2 dakika bekle
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
