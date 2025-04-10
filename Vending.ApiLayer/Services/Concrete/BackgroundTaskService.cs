using Microsoft.EntityFrameworkCore;
using Vending.ApiLayer.Services.Abstract;
using Vending.DataAccessLayer.Concrete;

public class BackgroundTaskService : IBackgroundTaskService
{
    private readonly ILogger<BackgroundTaskService> _logger;
    private readonly VendingContext _context;

    public BackgroundTaskService(ILogger<BackgroundTaskService> logger, VendingContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task GenerateInvoiceAndSendEmailAsync(InvoiceViewModel invoiceModel)
    {
        try
        {
            _logger.LogInformation("Fatura PDF oluşturuluyor...");

            // Razor + DinkToPdf ile PDF oluşturma işlemi
            var invoiceService = new InvoiceService();
            var pdfBytes = await invoiceService.GenerateInvoicePdfAsync(invoiceModel);

            _logger.LogInformation("Fatura PDF oluşturuldu.");

            // SMTP ile e-posta gönderme
            var emailService = new EmailService();
            await emailService.SendInvoiceEmailAsync(invoiceModel.Email, invoiceModel.UserName, pdfBytes);

            _logger.LogInformation("Fatura e-postası gönderildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatura oluşturma veya e-posta gönderme işlemi sırasında bir hata oluştu.");
        }
    }
    public async Task SendPendingInvoicesAsync()
    {
        var pendingOrders = await _context.Orders
            .Include(o => o.Product)
            .Include(o => o.Vend)
            .Include(o => o.AppUser)
            .Where(o => !o.IsInvoiceSent)
            .ToListAsync();

        foreach (var order in pendingOrders)
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

            try
            {
                await GenerateInvoiceAndSendEmailAsync(invoiceModel);
                order.IsInvoiceSent = true;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Fatura başarıyla gönderildi: {OrderId}", order.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura gönderilirken hata oluştu: {OrderId}", order.OrderId);
            }
        }
    }

}