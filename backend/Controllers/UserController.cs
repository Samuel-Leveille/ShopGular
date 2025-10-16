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
    public async Task<IActionResult> GetProductById(long id)
    {
        ProductDto? product = await _userService.GetProductById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        UserDto? user = await _userService.Login(dto);
        if (user == null) return Unauthorized();
        return Ok(user);
    }
}