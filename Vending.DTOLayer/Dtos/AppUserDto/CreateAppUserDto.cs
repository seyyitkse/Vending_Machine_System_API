using System.ComponentModel.DataAnnotations;

namespace Vending.DtoLayer.Dtos.AppUserDto
{
    public class CreateAppUserDto
    {
        [Required]
        [StringLength(50)]
        [EmailAddress]
        public string? Mail { get; set; }
        [Required]
        [StringLength(50)]
        [MinLength(5)]
        public string? Password { get; set; }
        [Required]
        [StringLength(50)]
        [MinLength(5)]
        public string? ConfirmPassword { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        public int DepartmentID { get; set; }
    }
}
