using ShopGular.Backend.Models;
namespace ShopGular.backend.Models;
public class Seller : User
{
    public List<Product>? ProductsForSale;

    public Seller(string name, string email, string password, List<Product> productsForSale) : base(name, email, password)
    {
        ProductsForSale = productsForSale;
    }
}