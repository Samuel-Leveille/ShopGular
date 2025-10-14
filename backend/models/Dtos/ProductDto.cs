using ShopGular.Backend.Enums;
namespace ShopGular.Backend.Models.Dtos;
public record ProductDto(long Id, string Title, string Description, double Price, string Category, string Image, int Quantity, int PurchaseQuantity, ProductTag Tag, DateTime DateOfSale);
