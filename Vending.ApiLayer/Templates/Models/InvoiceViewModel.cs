public class InvoiceViewModel
{
    public int OrderId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
    public string VendName { get; set; }
    public DateTime OrderDate { get; set; }
}
