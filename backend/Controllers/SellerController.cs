using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;

namespace ShopGular.Backend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SellerController : ControllerBase
{

    private readonly SellerService _sellerService;
    private readonly UserService _userService;

    public SellerController(SellerService sellerService, UserService userService)
    {
        _sellerService = sellerService;
        _userService = userService;
    }

    [HttpPost("add-product")]
    public IActionResult AddProduct(CreateProductDto product)
    {
        ProductDto? productDto = _sellerService.AddProduct(product);
        return CreatedAtAction(nameof(_userService.GetProductById), new { id = productDto?.Id }, productDto);
    }

    [HttpGet("{id}")]
    public IActionResult GetSellerById(long id)
    {
        SellerDto? sellerDto = _sellerService.GetSellerById(id);
        return Ok(sellerDto);
    }

    [HttpPost("signup")]
    public IActionResult SignUp(SignUpSellerDto dto)
    {
        SellerDto? sellerDto = _sellerService.SignUp(dto);
        return CreatedAtAction(nameof(GetSellerById), new { id = sellerDto?.Id }, sellerDto);
    }
}