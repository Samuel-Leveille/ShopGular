using ShopGular.Backend.Models;
using ShopGular.Backend.Models.Dtos;
namespace ShopGular.backend.Models;
public class Seller : User
{
    public List<Product>? ProductsForSale { get; set; } = new();

    public Seller(string name, string email, string password) : base(name, email, password) { }

    public Seller(string name, string email, string password, List<Product> productsForSale) : base(name, email, password)
    {
        ProductsForSale = productsForSale;
    }

    public static Seller SignUpDtoToEntity(SignUpSellerDto dto)
    {
        return new Seller(dto.Name, dto.Email, dto.Password);
    }

    public static SellerDto ToDto(Seller seller)
    {
        return new SellerDto(seller.Id, seller.Name, seller.Email, seller.ProductsForSale);
    }
}