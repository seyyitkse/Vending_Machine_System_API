namespace Vending.ApiLayer.Services.Abstract
{
    public interface IBackgroundTaskService
    {
        Task GenerateInvoiceAndSendEmailAsync(InvoiceViewModel invoiceModel);
    }
}
