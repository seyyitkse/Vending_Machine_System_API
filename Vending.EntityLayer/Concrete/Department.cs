using System.ComponentModel.DataAnnotations;

namespace Vending.EntityLayer.Concrete
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }
        public string? Name { get; set; }
        public List<AppUser>? AppUsers { get; set; }

        public static implicit operator Department?(string? v)
        {
            throw new NotImplementedException();
        }
    }
}
