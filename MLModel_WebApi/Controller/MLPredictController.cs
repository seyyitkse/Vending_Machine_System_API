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
    }
}