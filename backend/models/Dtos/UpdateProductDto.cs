namespace ShopGular.Backend.Models.Dtos;

public record UpdateProductDto(
    string Title,
    string Description,
    int Quantity,
    string? ImagePath);

