// OrderPrediction.cs
using Microsoft.ML.Data;

public class OrderPrediction
{
    [ColumnName("Score")]
    public float PredictedQuantity { get; set; }

    // Zaman serisi veya anomali tespiti için ek özellikler
    public float[] ForecastedQuantities { get; set; }
    public bool IsAnomaly { get; set; }
    public float AnomalyScore { get; set; }
}