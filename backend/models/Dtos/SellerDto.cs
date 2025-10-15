namespace ShopGular.Backend.Models.Dtos;
public record SellerDto(long Id, string Name, string Email, List<Product>? ProductsForSale);