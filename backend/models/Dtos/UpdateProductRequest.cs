using Microsoft.AspNetCore.Http;

namespace ShopGular.Backend.Models.Dtos;

public class UpdateProductRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Quantity { get; set; }
    public IFormFile? ImageFile { get; set; }
}

