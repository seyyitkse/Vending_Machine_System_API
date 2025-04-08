using Microsoft.AspNetCore.Mvc; // ASP.NET Core MVC için gerekli namespace
using System.Text; // Encoding sınıfı için gerekli namespace
using System.IO; // File operations
using System.Linq; // LINQ operations
using System.Collections.Generic; // List usage

namespace Vending.ApiLayer.Controllers
{
    [ApiController] // Bu sınıfın bir API controller olduğunu belirtir
    [Route("api/[controller]")] // API endpoint'inin yolunu belirler, bu durumda "api/logs"
    public class LogsController : ControllerBase
    {
        [HttpGet("get-logs")] // HTTP GET isteği için bu metodu işaretler
        public IActionResult GetLogs() // Logları almak için metod
        {
            var logDirectoryPath = "logs/";  // Log dosyalarının bulunduğu klasör yolu
            var logEntries = new List<object>(); // Tüm logları tutacak liste

            try
            {
                // Klasördeki tüm dosyaları al
                var logFiles = Directory.GetFiles(logDirectoryPath, "*.txt");

                foreach (var logFilePath in logFiles)
                {
                    // FileStream ile dosyayı paylaşımlı olarak açıyoruz
                    using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        var logContents = reader.ReadToEnd(); // Log dosyasını oku
                        var logLines = logContents.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) // Satırlara ayır
                                                  .Select(line =>
                                                  {
                                                      // Satırdaki karakterleri temizle (örneğin, sonundaki "\r")
                                                      line = line.TrimEnd('\r');

                                                      // Satırı parçalayarak JSON objesi oluştur
                                                      // Saat damgası ve log seviyesini ayırmak için " " ile split yap
                                                      var timestampEndIndex = line.IndexOf('[');
                                                      if (timestampEndIndex == -1)
                                                      {
                                                          return null; // Log formatı hatalı
                                                      }

                                                      var timestamp = line.Substring(0, timestampEndIndex).Trim(); // Zaman damgasını al
                                                      var levelEndIndex = line.IndexOf(']', timestampEndIndex);
                                                      if (levelEndIndex == -1)
                                                      {
                                                          return null; // Log formatı hatalı
                                                      }

                                                      var logLevel = line.Substring(timestampEndIndex, levelEndIndex - timestampEndIndex + 1); // Log seviyesini al
                                                      var message = line.Substring(levelEndIndex + 1).Trim(); // Mesajı al

                                                      return new
                                                      {
                                                          Dosya = Path.GetFileName(logFilePath), // Hangi dosyadan geldiğini ekle
                                                          Timestamp = timestamp, // Zaman damgasını al
                                                          Level = logLevel, // Log seviyesini al
                                                          Message = message // Mesajı al
                                                      };
                                                  }).Where(entry => entry != null) // Null entry'leri filtrele
                                                  .ToList(); // Tüm logları bir listeye dönüştür

                        logEntries.AddRange(logLines); // Bu dosyadan gelen logları ana listeye ekle
                    }
                }

                // Eğer log kaydı yoksa 204 No Content döndür
                if (!logEntries.Any())
                {
                    return NoContent();
                }

                return Ok(logEntries); // Logları JSON formatında döndür
            }
            catch (DirectoryNotFoundException) // Klasör bulunamazsa
            {
                return NotFound("Log klasörü bulunamadı."); // 404 Not Found döndür
            }
            catch (IOException ex) // Dosya okuma hatası olursa
            {
                return StatusCode(500, $"Dosya okuma hatası: {ex.Message}"); // 500 Internal Server Error döndür
            }
        }
    }
}


//using Microsoft.AspNetCore.Mvc; // ASP.NET Core MVC için gerekli namespace
//using System.Text; // Encoding sınıfı için gerekli namespace
//using System.IO; // File operations
//using System.Linq; // LINQ operations
//using System.Collections.Generic; // List usage
//using Vending.BusinessLayer.Abstract; // ILogService için gerekli namespace

//namespace Vending.ApiLayer.Controllers
//{
//    [ApiController] // Bu sınıfın bir API controller olduğunu belirtir
//    [Route("api/[controller]")] // API endpoint'inin yolunu belirler, bu durumda "api/logs"
//    public class LogsController : ControllerBase
//    {
//        private readonly ILogService _logService;

//        public LogsController(ILogService logService)
//        {
//            _logService = logService;
//        }

//        [HttpGet("get-logs")] // HTTP GET isteği için bu metodu işaretler
//        public IActionResult GetLogs() // Logları almak için metod
//        {
//            var logDirectoryPath = "logs/";  // Log dosyalarının bulunduğu klasör yolu
//            var logEntries = new List<object>(); // Tüm logları tutacak liste

//            try
//            {
//                // Klasördeki tüm dosyaları al
//                var logFiles = Directory.GetFiles(logDirectoryPath, "*.txt");

//                foreach (var logFilePath in logFiles)
//                {
//                    // FileStream ile dosyayı paylaşımlı olarak açıyoruz
//                    using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
//                    using (var reader = new StreamReader(fileStream, Encoding.UTF8))
//                    {
//                        var logContents = reader.ReadToEnd(); // Log dosyasını oku
//                        var logLines = logContents.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) // Satırlara ayır
//                                                  .Select(line =>
//                                                  {
//                                                      // Satırdaki karakterleri temizle (örneğin, sonundaki "\r")
//                                                      line = line.TrimEnd('\r');

//                                                      // Satırı parçalayarak JSON objesi oluştur
//                                                      // Saat damgası ve log seviyesini ayırmak için " " ile split yap
//                                                      var timestampEndIndex = line.IndexOf('[');
//                                                      if (timestampEndIndex == -1)
//                                                      {
//                                                          return null; // Log formatı hatalı
//                                                      }

//                                                      var timestamp = line.Substring(0, timestampEndIndex).Trim(); // Zaman damgasını al
//                                                      var levelEndIndex = line.IndexOf(']', timestampEndIndex);
//                                                      if (levelEndIndex == -1)
//                                                      {
//                                                          return null; // Log formatı hatalı
//                                                      }

//                                                      var logLevel = line.Substring(timestampEndIndex, levelEndIndex - timestampEndIndex + 1); // Log seviyesini al
//                                                      var message = line.Substring(levelEndIndex + 1).Trim(); // Mesajı al

//                                                      // Log entry'yi veri tabanına yaz
//                                                      _logService.LogInformation(message);

//                                                      return new
//                                                      {
//                                                          Dosya = Path.GetFileName(logFilePath), // Hangi dosyadan geldiğini ekle
//                                                          Timestamp = timestamp, // Zaman damgasını al
//                                                          Level = logLevel, // Log seviyesini al
//                                                          Message = message // Mesajı al
//                                                      };
//                                                  }).Where(entry => entry != null) // Null entry'leri filtrele
//                                                  .ToList(); // Tüm logları bir listeye dönüştür

//                        logEntries.AddRange(logLines); // Bu dosyadan gelen logları ana listeye ekle
//                    }
//                }

//                // Eğer log kaydı yoksa 204 No Content döndür
//                if (!logEntries.Any())
//                {
//                    return NoContent();
//                }

//                return Ok(logEntries); // Logları JSON formatında döndür
//            }
//            catch (DirectoryNotFoundException) // Klasör bulunamazsa
//            {
//                return NotFound("Log klasörü bulunamadı."); // 404 Not Found döndür
//            }
//            catch (IOException ex) // Dosya okuma hatası olursa
//            {
//                return StatusCode(500, $"Dosya okuma hatası: {ex.Message}"); // 500 Internal Server Error döndür
//            }
//        }
//    }
//}

//using Microsoft.AspNetCore.Mvc; // ASP.NET Core MVC için gerekli namespace
//using Vending.BusinessLayer.Abstract; // ILogService için gerekli namespace
//using Vending.ApiLayer.Models; // LogErrorModel için gerekli namespace

//namespace Vending.ApiLayer.Controllers
//{
//    [ApiController] // Bu sınıfın bir API controller olduğunu belirtir
//    [Route("api/[controller]")] // API endpoint'inin yolunu belirler, bu durumda "api/logs"
//    public class LogsController : ControllerBase
//    {
//        private readonly ILogService _logService;

//        public LogsController(ILogService logService)
//        {
//            _logService = logService;
//        }

//        [HttpPost("log-information")] // HTTP POST isteği için bu metodu işaretler
//        public IActionResult LogInformation([FromBody] string message) // Bilgi loglarını eklemek için metod
//        {
//            _logService.LogInformation(message);
//            return Ok("Information log added to the database.");
//        }

//        [HttpPost("log-warning")] // HTTP POST isteği için bu metodu işaretler
//        public IActionResult LogWarning([FromBody] string message) // Uyarı loglarını eklemek için metod
//        {
//            _logService.LogWarning(message);
//            return Ok("Warning log added to the database.");
//        }

//        [HttpPost("log-error")] // HTTP POST isteği için bu metodu işaretler
//        public IActionResult LogError([FromBody] LogErrorModel model) // Hata loglarını eklemek için metod
//        {
//            var exception = new Exception(model.Exception);
//            _logService.LogError(model.Message, exception);
//            return Ok("Error log added to the database.");
//        }

//        [HttpGet("get-logs")] // HTTP GET isteği için bu metodu işaretler
//        public IActionResult GetLogs() // Logları almak için metod
//        {
//            // Logları veri tabanından al
//            var logs = _logService.GetAllLogs();
//            if (!logs.Any())
//            {
//                return NoContent();
//            }
//            return Ok(logs);
//        }
//    }
//}

