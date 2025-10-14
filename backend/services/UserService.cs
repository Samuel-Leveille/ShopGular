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

    public ProductDto? GetProductById(long id)
    {
        Product? product = null;
        try
        {
            product = _context.Products.Find(id);
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
}