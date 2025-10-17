using ShopGular.Backend.Models;
using ShopGular.Backend.Models.Dtos;
namespace ShopGular.backend.Models;
public class Client : User
{
    public string FirstName { get; set; } = string.Empty;
    public List<Product>? ProductsBought { get; set; } = new();
    public List<Product>? ProductsInShoppingCart { get; set; } = new();

    public Client(string firstName, string name, string email, string password) : base(name, email, password)
    {
        FirstName = firstName;
    }

    public Client(string firstName, string name, string email, string password, List<Product> productsBought, List<Product> productsInShoppingCart) : base(name, email, password)
    {
        FirstName = firstName;
        ProductsBought = productsBought;
        ProductsInShoppingCart = productsInShoppingCart;
    }

    public static Client SignUpDtoToEntity(SignUpClientDto dto)
    {
        return new Client(dto.FirstName, dto.Name, dto.Email, dto.Password);
    }

    public static ClientDto ToDto(Client client)
    {
        return new ClientDto(client.Id, client.FirstName, client.Name, client.Email, client.ProductsBought, client.ProductsInShoppingCart);
    }
}