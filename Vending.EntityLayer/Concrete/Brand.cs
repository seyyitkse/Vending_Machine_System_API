namespace Vending.EntityLayer.Concrete
{
    public class Brand
    {
        public int BrandId { get; set; }
        public string Name { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; }

        public Brand()
        {
            Products = new HashSet<Product>();
        }
    }
}
