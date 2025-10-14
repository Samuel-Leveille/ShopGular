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

    public ProductDto AddProduct(CreateProductDto product)
    {
        Product entity = Product.ToEntityNewProduct(product);
        _context.Products.Add(entity);
        _context.SaveChanges();
        return Product.ToDto(entity);
    }
}