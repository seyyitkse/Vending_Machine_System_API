//using DinkToPdf.Contracts;
//using DinkToPdf;
//using RazorLight;

//public class InvoiceService
//{
//    private readonly RazorLightEngine _razorEngine;
//    private readonly IConverter _pdfConverter;

//    public InvoiceService()
//    {
//        _razorEngine = new RazorLightEngineBuilder()
//            .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
//            .UseMemoryCachingProvider()
//            .Build();

//        _pdfConverter = new SynchronizedConverter(new PdfTools());
//    }
//    private static readonly object _pdfLock = new object();

//    public async Task<byte[]> GenerateInvoicePdfAsync(InvoiceViewModel model)
//    {
//        try
//        {
//            // Razor view'dan HTML render al
//            string htmlContent = await _razorEngine.CompileRenderAsync("InvoiceTemplate.cshtml", model);

//            // PDF ayarları
//            var doc = new HtmlToPdfDocument()
//            {
//                GlobalSettings = new GlobalSettings
//                {
//                    PaperSize = PaperKind.A4,
//                    Orientation = Orientation.Portrait,
//                },
//                Objects = {
//                new ObjectSettings
//                {
//                    HtmlContent = htmlContent,
//                    WebSettings = { DefaultEncoding = "utf-8" }
//                }
//            }
//            };

//            // Ensure thread safety with lock
//            byte[] pdfBytes;
//            lock (_pdfLock)
//            {
//                pdfBytes = _pdfConverter.Convert(doc);
//            }

//            return pdfBytes;
//        }
//        catch (Exception ex)
//        {
//            // Log the exception for debugging
//            throw;
//        }
//    }

//    //buradaki ilk metot dinktopdf kullanılarak yapılmıştır.
//    //public async Task<byte[]> GenerateInvoicePdfAsync(InvoiceViewModel model)
//    //{
//    //    try
//    //    {
//    //        // Razor view'dan HTML render al
//    //        string htmlContent = await _razorEngine.CompileRenderAsync("InvoiceTemplate.cshtml", model);

//    //        // PDF ayarları
//    //        var doc = new HtmlToPdfDocument()
//    //        {
//    //            GlobalSettings = new GlobalSettings
//    //            {
//    //                PaperSize = PaperKind.A4,
//    //                Orientation = Orientation.Portrait,
//    //            },
//    //            Objects = {
//    //            new ObjectSettings
//    //            {
//    //                HtmlContent = htmlContent,
//    //                WebSettings = { DefaultEncoding = "utf-8" }
//    //            }
//    //        }
//    //        };

//    //        // PDF oluşturma işlemini asenkron hale getir
//    //        var pdfBytes = await Task.Run(() => _pdfConverter.Convert(doc));

//    //        return pdfBytes;
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        throw;  // Hata durumunda loglama yapabilir ve dışarıya hata fırlatabilirsiniz.
//    //    }
//    //}

//    //public async Task<byte[]> GenerateInvoicePdfAsync(InvoiceViewModel model)
//    //{
//    //    // Razor view'dan HTML render al
//    //    string htmlContent = await _razorEngine.CompileRenderAsync("InvoiceTemplate.cshtml", model);

//    //    // PDF ayarları
//    //    var doc = new HtmlToPdfDocument()
//    //    {
//    //        GlobalSettings = new GlobalSettings
//    //        {
//    //            PaperSize = PaperKind.A4,
//    //            Orientation = Orientation.Portrait,
//    //        },
//    //        Objects = {
//    //            new ObjectSettings
//    //            {
//    //                HtmlContent = htmlContent,
//    //                WebSettings = { DefaultEncoding = "utf-8" }
//    //            }
//    //        }
//    //    };

//    //    return _pdfConverter.Convert(doc);
//    //}


//}

using DinkToPdf;
using DinkToPdf.Contracts;
using RazorLight;

public class InvoiceService
{
    private readonly RazorLightEngine _razorEngine;
    private static readonly IConverter _pdfConverter = new SynchronizedConverter(new PdfTools());
    private static readonly object _pdfLock = new object();

    public InvoiceService()
    {
        _razorEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(InvoiceViewModel model)
    {
        try
        {
            string htmlContent = await _razorEngine.CompileRenderAsync("InvoiceTemplate.cshtml", model);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects = {
                    new ObjectSettings
                    {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            lock (_pdfLock)
            {
                return _pdfConverter.Convert(doc);
            }
        }
        catch (Exception ex)
        {
            // Logging yapılabilir
            throw;
        }
    }
}

