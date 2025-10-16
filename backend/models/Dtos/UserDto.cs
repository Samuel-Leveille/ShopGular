namespace ShopGular.Backend.Models.Dtos;
public record UserDto(long Id, string Name, string? Firstname, string Email, List<Product>? ProductsForSale);