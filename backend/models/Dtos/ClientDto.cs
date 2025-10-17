namespace ShopGular.Backend.Models.Dtos;
public record ClientDto(long Id, string FirstName, string Name, string Email, List<Product>? ProductsBought, List<Product>? ProductsInShoppingCart);