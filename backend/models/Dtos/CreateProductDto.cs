using ShopGular.Backend.Enums;

public record CreateProductDto(string Title, string Description, double Price, string Category, string Image, int Quantity, int PurchaseQuantity, ProductTag Tag, DateTime DateOfSale);