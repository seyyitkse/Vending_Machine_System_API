using System.ComponentModel.DataAnnotations;

namespace Vending.EntityLayer.Concrete
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Title { get; set; }
        public bool Completed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
