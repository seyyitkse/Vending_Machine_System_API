using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vending.EntityLayer.Concrete;

namespace Vending.DataAccessLayer.Concrete
{
    public class VendingContext : IdentityDbContext<AppUser, AppRole, int>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=DESKTOP-6B43INH\\SQLEXPRESS;database=DbVending;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;encrypt=false");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One Department can have many AppUsers
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.AppUsers)
                .HasForeignKey(u => u.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            // One Category can have many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            // One Brand can have many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Vend> Vends { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
    }
}
