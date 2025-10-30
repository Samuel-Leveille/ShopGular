using Microsoft.EntityFrameworkCore;
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

    public async Task<UserDto?> Login(LoginDto dto)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !PasswordHashing.VerifyPassword(dto.Password, user.Password))
        {
            Console.WriteLine("Utilisateur non autorisé à ce connecter : mauvais email et/ou mauvais mot de passe");
            return null;
        }
        return user != null ? User.ToDto(user) : null;
    }

    public async Task<User?> LoginEntity(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !PasswordHashing.VerifyPassword(dto.Password, user.Password))
        {
            return null;
        }
        return user;
    }

    public async Task<User?> GetUserById(long id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
}