using Microsoft.EntityFrameworkCore;
using ShopGular.Backend.Models;
namespace ShopGular.Backend.DbContext;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}