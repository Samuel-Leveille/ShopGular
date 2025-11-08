using Microsoft.EntityFrameworkCore;
using ShopGular.backend.Models;
using ShopGular.Backend.Models;
using ShopGular.Backend.Models.Dtos;

namespace ShopGular.Backend.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> GetProductById(long id)
    {
        Product? product = null;
        try
        {
            product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                Console.WriteLine($"Produit avec id {id} introuvable");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'obtention d'un produit par l'id : {ex.Message}");
        }

        return product != null ? Product.ToDto(product) : null;
    }

    public async Task<User?> LoginEntity(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !PasswordHashing.VerifyPassword(dto.Password, user.Password))
        {
            return null;
        }

        return await HydrateDerivedUserAsync(user);
    }

    public async Task<User?> GetUserById(long id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return null;
        }

        return await HydrateDerivedUserAsync(user);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return null;
        }

        return await HydrateDerivedUserAsync(user);
    }

    public async Task<(string type, object dto)> MapUserToDtoAsync(User user)
    {
        var hydrated = await HydrateDerivedUserAsync(user);
        return hydrated switch
        {
            Client client => ("client", Client.ToDto(client)),
            Seller seller => ("seller", Seller.ToDto(seller)),
            _ => ("user", Models.User.ToDto(hydrated))
        };
    }

    public async Task<User> CreateClientFromGoogleAsync(string email, string firstName, string lastName)
    {
        var password = PasswordHashing.HashPassword(Guid.NewGuid().ToString("N"));
        var client = new Client(firstName, lastName, email, password);
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<User> CreateSellerFromGoogleAsync(string email, string sellerName)
    {
        var password = PasswordHashing.HashPassword(Guid.NewGuid().ToString("N"));
        var seller = new Seller(sellerName, email, password);
        _context.Sellers.Add(seller);
        await _context.SaveChangesAsync();
        return seller;
    }

    private async Task<User> HydrateDerivedUserAsync(User user)
    {
        if (user is Client || user is Seller)
        {
            return user;
        }

        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == user.Id);
        if (client != null)
        {
            return client;
        }

        var seller = await _context.Sellers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == user.Id);
        if (seller != null)
        {
            return seller;
        }

        return user;
    }
}