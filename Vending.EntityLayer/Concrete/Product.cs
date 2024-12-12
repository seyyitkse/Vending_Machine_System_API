namespace Vending.EntityLayer.Concrete
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; } // Make Category nullable

        public int BrandId { get; set; }
        public Brand? Brand { get; set; } // Make Brand nullable
    }
}