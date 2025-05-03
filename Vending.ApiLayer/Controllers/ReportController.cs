//using Microsoft.AspNetCore.Mvc;
//using Vending.ApiLayer.Services.Reports;

//namespace Vending.ApiLayer.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ReportController : ControllerBase
//    {
//        private readonly DailySpendingReportService _reportService;

//        public ReportController(DailySpendingReportService reportService)
//        {
//            _reportService = reportService;
//        }

//        [HttpGet("daily-spending-report")]
//        public async Task<IActionResult> GetDailySpendingReport()
//        {
//            var pdfBytes = await _reportService.GenerateDailySpendingReportAsync();

//            return File(pdfBytes, "application/pdf", $"GunlukRapor-{DateTime.UtcNow:yyyy-MM-dd}.pdf");
//        }


//    }
//}

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vending.ApiLayer.Services.Concrete;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportController(ReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Günlük harcama raporunu PDF olarak döner.
    /// </summary>
    [HttpGet("daily-report")]
    public async Task<IActionResult> GetDailyReport()
    {
        var pdfBytes = await _reportService.GenerateDailyReportAsync();
        return File(pdfBytes, "application/pdf", $"GunlukRapor-{DateTime.UtcNow:yyyy-MM-dd}.pdf");
    }

    /// <summary>
    /// Haftalık harcama raporunu PDF olarak döner.
    /// </summary>
    [HttpGet("weekly-report")]
    public async Task<IActionResult> GetWeeklyReport()
    {
        var pdfBytes = await _reportService.GenerateWeeklyReportAsync();
        return File(pdfBytes, "application/pdf", $"HaftalikRapor-{DateTime.UtcNow:yyyy-MM-dd}.pdf");
    }

    /// <summary>
    /// Aylık harcama raporunu PDF olarak döner.
    /// </summary>
    [HttpGet("monthly-report")]
    public async Task<IActionResult> GetMonthlyReport()
    {
        var pdfBytes = await _reportService.GenerateMonthlyReportAsync();
        return File(pdfBytes, "application/pdf", $"AylikRapor-{DateTime.UtcNow:yyyy-MM-dd}.pdf");
    }

    /// <summary>
    /// Belirli bir tarih aralığı için harcama raporunu PDF olarak döner.
    /// </summary>
    [HttpGet("custom-report")]
    public async Task<IActionResult> GetCustomReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var pdfBytes = await _reportService.GenerateCustomReportAsync(startDate, endDate);
        return File(pdfBytes, "application/pdf", $"OzelRapor-{startDate:dd-MM-yyyy}_to_{endDate:dd-MM-yyyy}.pdf");
    }

    /// <summary>
    /// Günlük raporu oluşturur ve admin kullanıcılara gönderir.
    /// </summary>
    [HttpPost("send-daily-report")]
    public async Task<IActionResult> SendDailyReport()
    {
        try
        {
            await _reportService.SendReportToAdminsAsync("daily");
            return Ok(new { Message = "Günlük rapor adminlere başarıyla gönderildi." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Rapor gönderimi sırasında bir hata oluştu.", Error = ex.Message });
        }
    }

    /// <summary>
    /// Aylık raporu oluşturur ve admin kullanıcılara gönderir.
    /// </summary>
    [HttpPost("send-monthly-report")]
    public async Task<IActionResult> SendMonthlyReport()
    {
        try
        {
            await _reportService.SendReportToAdminsAsync("monthly");
            return Ok(new { Message = "Aylık rapor adminlere başarıyla gönderildi." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Rapor gönderimi sırasında bir hata oluştu.", Error = ex.Message });
        }
    }
}

