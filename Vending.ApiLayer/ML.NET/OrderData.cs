namespace Vending.ApiLayer.ML.NET
{
    public class OrderData
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public long UserCode { get; set; }
        public int VendId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int ProductStock { get; set; } // VendProduct'ten
        public string ProductName { get; set; }
        public string VendName { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        // Tahmin edilecek değer (örneğin gelecek hafta satış miktarı)
        public float PredictedQuantity { get; set; }
    }
}
