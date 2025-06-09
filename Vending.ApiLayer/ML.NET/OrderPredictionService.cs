// OrderPredictionService.cs
using Microsoft.ML;
using Vending.ApiLayer.ML.NET;

public class OrderPredictionService
{
    private readonly PredictionEngine<OrderData, OrderPrediction> _predictionEngine;
    private readonly MLContext _mlContext;

    public OrderPredictionService(MLContext mlContext, ITransformer model)
    {
        _mlContext = mlContext;
        _predictionEngine = mlContext.Model.CreatePredictionEngine<OrderData, OrderPrediction>(model);
    }

    public OrderPrediction PredictOrder(OrderData input)
    {
        return _predictionEngine.Predict(input);
    }

    public List<OrderPrediction> PredictOrders(List<OrderData> inputs)
    {
        return inputs.Select(input => _predictionEngine.Predict(input)).ToList();
    }
}