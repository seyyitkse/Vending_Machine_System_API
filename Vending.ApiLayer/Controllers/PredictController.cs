
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Vending.ApiLayer.ML.NET;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEngine<OrderData, OrderPrediction> _predictionEngine;

        public PredictController()
        {
            // Modeli yükle
            var mlContext = new MLContext();
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "Models", "order-model.zip");

            ITransformer mlModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                mlModel = mlContext.Model.Load(stream, out _);
            }

            _predictionEngine = mlContext.Model.CreatePredictionEngine<OrderData, OrderPrediction>(mlModel);
        }

        [HttpPost]
        public IActionResult Predict([FromBody] OrderData input)
        {
            try
            {
                // Tahmin yap
                var prediction = _predictionEngine.Predict(input);

                // Sonucu döndür
                return Ok(new
                {
                    ProductId = input.ProductId,
                    VendId = input.VendId,
                    PredictedQuantity = prediction.PredictedQuantity,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Prediction error: {ex.Message}");
            }
        }
    }
}
