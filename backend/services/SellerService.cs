using ShopGular.backend.Models;
using ShopGular.Backend.Models;
using ShopGular.Backend.Models.Dtos;
namespace ShopGular.Backend.Services;
public class SellerService
{

    private readonly AppDbContext _context;

    public SellerService(AppDbContext context)
    {
        _context = context;
    }

    public ProductDto? AddProduct(CreateProductDto product)
    {
        Product? entity = null;
        try
        {
            entity = Product.ToEntityNewProduct(product);
            _context.Products.Add(entity);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la tentative d'ajout d'un nouveau produit : {ex.Message}");
        }
        return entity != null ? Product.ToDto(entity) : null;
    }

    public SellerDto? SignUp(SignUpSellerDto dto)
    {
        Seller? seller = null;
        try
        {
            seller = Seller.SignUpDtoToEntity(dto);
            _context.Sellers.Add(seller);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la tentative d'ajout d'un nouvel utilisateur seller : {ex.Message}");
        }
        return seller != null ? Seller.ToDto(seller) : null;
    }

    public SellerDto? GetSellerById(long id)
    {
        Seller? seller = null;
        try
        {
            seller = _context.Sellers.Find(id);
            if (seller == null)
            {
                Console.WriteLine($"User seller avec id {id} introuvable");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'obtention d'un utilisateur seller par son id : {ex.Message}");
        }
        return seller != null ? Seller.ToDto(seller) : null;
    }
}