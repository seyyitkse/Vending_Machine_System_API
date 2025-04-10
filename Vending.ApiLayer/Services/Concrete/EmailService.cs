using System.Net.Mail;
using System.Net;

public class EmailService
{
    public async Task SendInvoiceEmailAsync(string toEmail, string toName, byte[] pdfBytes)
    {
        var fromAddress = new MailAddress("foodvendingmachines@gmail.com", "Food Vending Machines");
        var toAddress = new MailAddress(toEmail, toName);
        const string subject = "Satın Aldığınız Ürünün Faturası";
        const string body = "Merhaba, ürün satın alma işleminizin faturası ekte PDF olarak sunulmuştur. Teşekkür ederiz.";

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("foodsvendingmachines@gmail.com", "ukniskyervovwrlc"), // uygulama şifresi!
            Timeout = 20000 // 20 saniye
        };

        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            // PDF ekini oluştur
            message.Attachments.Add(new Attachment(
                new MemoryStream(pdfBytes),
                "Fatura.pdf",
                "application/pdf"
            ));

            await smtp.SendMailAsync(message);
        }
    }
}