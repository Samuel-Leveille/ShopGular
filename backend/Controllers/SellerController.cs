using System.Globalization;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Enums;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;

namespace ShopGular.Backend.Controllers;

[ApiController]
[Route("api/user/[controller]")]
public class SellerController : ControllerBase
{
    private readonly SellerService _sellerService;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public SellerController(SellerService sellerService, UserService userService, IWebHostEnvironment environment)
    {
        _sellerService = sellerService;
        _userService = userService;
        _environment = environment;
    }

    [Authorize]
    [HttpPost("products")]
    public async Task<IActionResult> AddProduct([FromForm] CreateProductRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserById(userId);
        if (user is not backend.Models.Seller seller)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            string.IsNullOrWhiteSpace(request.Category) ||
            string.IsNullOrWhiteSpace(request.Price) ||
            string.IsNullOrWhiteSpace(request.Quantity) ||
            request.ImageFile == null || request.ImageFile.Length == 0)
        {
            return BadRequest(new { message = "Les informations du produit sont incomplètes." });
        }

        if (!double.TryParse(request.Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice) ||
            parsedPrice <= 0 ||
            !int.TryParse(request.Quantity, out var parsedQuantity) ||
            parsedQuantity < 0)
        {
            return BadRequest(new { message = "Le prix ou la quantité est invalide." });
        }

        var imagePath = await SaveImageAsync(request.ImageFile);

        var dto = new CreateProductDto(
            request.Title,
            request.Description,
            parsedPrice,
            request.Category,
            imagePath,
            parsedQuantity,
            ParseNullableInt(request.PurchaseQuantity),
            ParseTag(request.Tag),
            ParseDate(request.DateOfSale));

        var productDto = await _sellerService.AddProductAsync(seller.Id, dto);
        if (productDto == null)
        {
            return BadRequest(new { message = "Impossible de créer le produit." });
        }

        return Ok(productDto);
    }

    [Authorize]
    [HttpPut("products/{productId:long}")]
    public async Task<IActionResult> UpdateProduct(long productId, [FromForm] UpdateProductRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserById(userId);
        if (user is not backend.Models.Seller seller)
        {
            return Forbid();
        }

        var title = request.Title?.Trim();
        var description = request.Description?.Trim();
        var quantityRaw = request.Quantity?.Trim();

        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(description) ||
            string.IsNullOrWhiteSpace(quantityRaw))
        {
            return BadRequest(new { message = "Les champs Titre, Description et Quantité sont requis." });
        }

        if (!int.TryParse(quantityRaw, out var quantity) || quantity < 0)
        {
            return BadRequest(new { message = "La quantité doit être un nombre entier positif ou nul." });
        }

        string? imagePath = null;
        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            imagePath = await SaveImageAsync(request.ImageFile);
            if (imagePath == null)
            {
                return BadRequest(new { message = "Impossible de sauvegarder la nouvelle image." });
            }
        }

        var dto = new UpdateProductDto(title, description, quantity, imagePath);
        var updated = await _sellerService.UpdateProductAsync(seller.Id, productId, dto);
        if (updated == null)
        {
            return NotFound(new { message = "Produit introuvable ou accès refusé." });
        }

        return Ok(updated);
    }
    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value, out var result) ? result : null;
    }

    private static ProductTag ParseTag(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<ProductTag>(value, out var tag))
        {
            return tag;
        }
        return ProductTag.InStock;
    }

    private static DateTime ParseDate(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            DateTime.TryParse(value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var date))
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
        return DateTime.UtcNow;
    }

    [Authorize]
    [HttpGet("products")]
    public async Task<IActionResult> GetMyProducts()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserById(userId);
        if (user is not backend.Models.Seller seller)
        {
            return Forbid();
        }

        var items = await _sellerService.GetProductsForSellerAsync(seller.Id);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSellerById(long id)
    {
        var sellerDto = await _sellerService.GetSellerByIdAsync(id);
        if (sellerDto == null)
        {
            return NotFound();
        }

        return Ok(sellerDto);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(SignUpSellerDto dto)
    {
        var sellerDto = await _sellerService.SignUpAsync(dto);
        if (sellerDto == null)
        {
            return Conflict(new { message = "Un compte avec cet email existe déjà." });
        }

        return CreatedAtAction(nameof(GetSellerById), new { id = sellerDto.Id }, sellerDto);
    }

    private bool TryGetUserId(out long userId)
    {
        userId = 0;
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return long.TryParse(sub, out userId);
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath!;

        var uploadFolder = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(uploadFolder);

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".png";
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var relativePath = $"/uploads/products/{fileName}";
        return relativePath;
    }
}
