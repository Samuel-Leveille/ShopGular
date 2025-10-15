using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;

namespace ShopGular.Backend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("product/{id}")]
    public IActionResult GetProductById(long id)
    {
        ProductDto? product = _userService.GetProductById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }
}