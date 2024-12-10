using Microsoft.AspNetCore.Identity;

namespace Vending.EntityLayer.Concrete
{
    public class AppUser:IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName{ get; set; }
        public int? DepartmentID { get; set; }
        public Department? Department { get; set; }
    }
}
