using Microsoft.AspNetCore.Mvc; // ASP.NET Core MVC için gerekli namespace
using System.Text; // Encoding sınıfı için gerekli namespace

namespace Vending.ApiLayer.Controllers
{
    [ApiController] // Bu sınıfın bir API controller olduğunu belirtir
    [Route("api/[controller]")] // API endpoint'inin yolunu belirler, bu durumda "api/logs"
    public class LogsController : ControllerBase
    {
        [HttpGet("get-logs")] // HTTP GET isteği için bu metodu işaretler
        public IActionResult GetLogs() // Logları almak için metod
        {
            var logFilePath = "logs/log-20241014.txt";  // Log dosyasının yolu

            try
            {
                // FileStream ile dosyayı paylaşımlı olarak açıyoruz
                using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    var logContents = reader.ReadToEnd(); // Log dosyasını oku
                    var logLines = logContents.Split('\n') // Log içeriğini satırlara ayır
                                              .Where(line => !string.IsNullOrWhiteSpace(line)) // Boş satırları filtrele
                                              .Select(line =>
                                              {
                                                  // Her satırı parçalayarak JSON objesi oluştur
                                                  var parts = line.Split(' '); // Boşluklardan ayır, log yapısına göre düzenle
                                                  return new
                                                  {
                                                      Timestamp = $"{parts[0]} {parts[1]}", // Zaman damgasını birleştir
                                                      Level = parts[3], // Log seviyesini al
                                                      Message = string.Join(" ", parts.Skip(4)) // Mesajı birleştir
                                                  };
                                              }).ToList(); // Tüm logları bir listeye dönüştür
                    return Ok(logLines); // Logları JSON formatında döndür
                }
            }
            catch (FileNotFoundException) // Dosya bulunamazsa
            {
                return NotFound("Log dosyası bulunamadı."); // 404 Not Found döndür
            }
            catch (IOException ex) // Dosya okuma hatası olursa
            {
                return StatusCode(500, $"Dosya okuma hatası: {ex.Message}"); // 500 Internal Server Error döndür
            }
        }
    }
}
