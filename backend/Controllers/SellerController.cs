using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;

namespace ShopGular.Backend.Controllers;

[ApiController]
[Route("api/user/[controller]")]
public class SellerController : ControllerBase
{
    private readonly SellerService _sellerService;
    private readonly UserService _userService;

    public SellerController(SellerService sellerService, UserService userService)
    {
        _sellerService = sellerService;
        _userService = userService;
    }

    [Authorize]
    [HttpPost("products")]
    public async Task<IActionResult> AddProduct(CreateProductDto product)
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

        var productDto = await _sellerService.AddProductAsync(seller.Id, product);
        if (productDto == null)
        {
            return BadRequest(new { message = "Impossible de créer le produit." });
        }

        return Ok(productDto);
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
}
