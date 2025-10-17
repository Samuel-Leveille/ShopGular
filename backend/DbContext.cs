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
    public DbSet<Client> Clients { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.BoughtByClient)
            .WithMany(c => c.ProductsBought)
            .HasForeignKey(p => p.BoughtByClientFK)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.InShoppingCartByClient)
            .WithMany(c => c.ProductsInShoppingCart)
            .HasForeignKey(p => p.InShoppingCartByClientFK)
            .OnDelete(DeleteBehavior.Cascade);
    }
}