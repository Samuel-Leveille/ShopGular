using ShopGular.Backend.Enums;
namespace ShopGular.Backend.Models.Dtos;
public record CreateProductDto(string Title, string Description, double Price, string Category, string Image, int Quantity, int PurchaseQuantity, ProductTag Tag, DateTime DateOfSale);