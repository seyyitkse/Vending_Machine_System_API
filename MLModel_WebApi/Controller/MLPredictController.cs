using Microsoft.AspNetCore.Mvc;

namespace MLModel_WebApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class MLPredictController : ControllerBase
    {
        [HttpPost]
        public IActionResult Predict([FromBody] MLModel.ModelInput input)
        {
            if (input == null)
                return BadRequest("Input cannot be null.");

            // Modeli kullanarak tahmin yap
            var result = MLModel.Predict(input);

            // Sonucu döndür
            return Ok(new
            {
                input.ProductId,
                input.UserCode,
                PredictedTotalPrice = result.Score
            });
        }

        [HttpGet("sample")]
        public IActionResult PredictSample()
        {
            // Örnek veriyi yükle
            var sampleData = new MLModel.ModelInput()
            {
                OrderId = 11F,
                ProductId = 1F,
                VendId = 1F,
                UserCode = 56038F,
                OrderDate = @"2025-04-04 15:25:24.6171571",
                TotalPrice = 1000F,
                IsInvoiceSent = 1F,
            };

            // Modeli yükle ve tahmin yap
            var result = MLModel.Predict(sampleData);

            // Sonucu döndür
            return Ok(new
            {
                sampleData.ProductId,
                sampleData.UserCode,
                PredictedTotalPrice = result.Score
            });
        }
        [HttpGet("predict")]
        public IActionResult PredictManual(
    [FromQuery] float orderId,
    [FromQuery] float productId,
    [FromQuery] float vendId,
    [FromQuery] float userCode,
    [FromQuery] string orderDate,
    [FromQuery] float totalPrice,
    [FromQuery] float isInvoiceSent)
        {
            var input = new MLModel.ModelInput
            {
                OrderId = orderId,
                ProductId = productId,
                VendId = vendId,
                UserCode = userCode,
                OrderDate = orderDate,
                TotalPrice = totalPrice,
                IsInvoiceSent = isInvoiceSent
            };

            var result = MLModel.Predict(input);

            return Ok(new
            {
                input.ProductId,
                input.UserCode,
                PredictedTotalPrice = result.Score
            });
        }
    }
}