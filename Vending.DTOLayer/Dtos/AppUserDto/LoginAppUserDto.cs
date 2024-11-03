using System.ComponentModel.DataAnnotations;

namespace Vending.DtoLayer.Dtos.AppUserDto
{
    public class LoginAppUserDto
    {
        [Required]
        [StringLength(50)]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [StringLength(50)]
        [MinLength(5)]
        public string? Password { get; set; }
    }
}
