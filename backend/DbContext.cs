using Microsoft.EntityFrameworkCore;
using ShopGular.backend.Models;
using ShopGular.Backend.Models;
namespace ShopGular.Backend;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Seller> Sellers { get; set; }
}