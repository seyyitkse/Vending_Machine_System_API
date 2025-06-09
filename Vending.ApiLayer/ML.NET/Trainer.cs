using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using Vending.ApiLayer.ML.NET;

public class ModelTrainer
{
    private MLContext _mlContext;

    public ModelTrainer()
    {
        _mlContext = new MLContext();
    }

    public ITransformer Train(List<OrderData> trainingData)
    {
        // 1. Veriyi yükle
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // 2. Pipeline oluştur
        var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(OrderData.ProductId),
                    nameof(OrderData.VendId),
                    nameof(OrderData.ProductStock))
                .Append(_mlContext.Regression.Trainers.Sdca());

        // 3. Modeli eğit
        var model = pipeline.Fit(dataView);

        return model;
    }

    public void SaveModel(ITransformer model, string modelPath)
    {
        _mlContext.Model.Save(model, null, modelPath);
    }

    public ITransformer LoadModel(string modelPath)
    {
        return _mlContext.Model.Load(modelPath, out _);
    }

    public RegressionMetrics Evaluate(ITransformer model, List<OrderData> testData)
    {
        var testDataView = _mlContext.Data.LoadFromEnumerable(testData);
        var predictions = model.Transform(testDataView);
        return _mlContext.Regression.Evaluate(predictions);
    }
}