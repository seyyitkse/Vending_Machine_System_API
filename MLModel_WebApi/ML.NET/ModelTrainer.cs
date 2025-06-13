using Microsoft.ML;

namespace Vending.ApiLayer.ML.NET
{
    // ModelTrainer.cs
    public class ModelTrainer
    {
        public void TrainAndSaveModel(List<OrderData> trainingData, string modelPath)
        {
            var mlContext = new MLContext();

            // Veriyi yükle
            var dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // Pipeline oluştur
            var pipeline = mlContext.Transforms.Concatenate("Features",
                        nameof(OrderData.ProductId),
                        nameof(OrderData.VendId),
                        nameof(OrderData.ProductStock))
                    .Append(mlContext.Regression.Trainers.Sdca());

            // Modeli eğit
            var model = pipeline.Fit(dataView);

            // Modeli kaydet
            mlContext.Model.Save(model, dataView.Schema, modelPath);
        }
    }
}
