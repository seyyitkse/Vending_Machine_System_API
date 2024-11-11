using System.ComponentModel.DataAnnotations;

namespace Vending.EntityLayer.Concrete
{
    public class Vend
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? LocationDescription { get; set; }
    }
}