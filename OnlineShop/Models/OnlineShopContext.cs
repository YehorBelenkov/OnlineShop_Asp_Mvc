using Microsoft.EntityFrameworkCore;
namespace OnlineShop.Models
{
    public class OnlineShopContext : DbContext
    {
        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<Products> Products { get; set; } = null!;
        public DbSet<Categories> Categories { get; set; } = null!;
        public DbSet<Roles> Roles { get; set; } = null!;
        public DbSet<Cards> Cards { get; set; } = null!;
        public OnlineShopContext(DbContextOptions<OnlineShopContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
