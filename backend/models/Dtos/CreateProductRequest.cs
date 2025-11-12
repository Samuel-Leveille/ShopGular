using Microsoft.AspNetCore.Http;
using ShopGular.Backend.Enums;

namespace ShopGular.Backend.Models.Dtos;

public class CreateProductRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string? PurchaseQuantity { get; set; }
    public string? Tag { get; set; }
    public string? DateOfSale { get; set; }
    public IFormFile? ImageFile { get; set; }
}

