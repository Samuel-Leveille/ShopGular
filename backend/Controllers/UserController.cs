using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;
using System.Security.Claims;

namespace ShopGular.Backend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

    private readonly UserService _userService;
    private readonly TokenService _tokenService;
    private readonly RefreshTokenService _refreshService;

    public UserController(UserService userService, TokenService tokenService, RefreshTokenService refreshService)
    {
        _userService = userService;
        _tokenService = tokenService;
        _refreshService = refreshService;
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
        var user = await _userService.LoginEntity(dto);
        if (user == null) return Unauthorized();

        var (access, accessExp) = _tokenService.CreateAccessToken(user);
        var (refresh, refreshExp) = await _tokenService.CreateAndStoreRefreshTokenAsync(user.Id);

        object typedUserDto;
        string type;
        if (user is ShopGular.backend.Models.Client client)
        {
            typedUserDto = ShopGular.backend.Models.Client.ToDto(client);
            type = "client";
        }
        else if (user is ShopGular.backend.Models.Seller seller)
        {
            typedUserDto = ShopGular.backend.Models.Seller.ToDto(seller);
            type = "seller";
        }
        else
        {
            typedUserDto = Models.User.ToDto(user);
            type = "user";
        }

        return Ok(new
        {
            type,
            user = typedUserDto,
            accessToken = access,
            accessTokenExpiresAtUtc = accessExp,
            refreshToken = refresh,
            refreshTokenExpiresAtUtc = refreshExp
        });
    }

    public record RefreshRequest(string RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.RefreshToken)) return BadRequest();
        var rt = await _refreshService.GetValidAsync(body.RefreshToken);
        if (rt == null) return Unauthorized();

        var user = await _userService.GetUserById(rt.UserId);
        if (user == null) return Unauthorized();

        var (access, accessExp) = _tokenService.CreateAccessToken(user);
        return Ok(new { accessToken = access, accessTokenExpiresAtUtc = accessExp });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest body)
    {
        if (!string.IsNullOrWhiteSpace(body.RefreshToken))
        {
            await _refreshService.RevokeAsync(body.RefreshToken);
        }
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(sub)) return Unauthorized();
        if (!long.TryParse(sub, out var userId)) return Unauthorized();
        var user = await _userService.GetUserById(userId);
        if (user == null) return Unauthorized();

        object typedUserDto;
        string type;
        if (user is ShopGular.backend.Models.Client client)
        {
            typedUserDto = ShopGular.backend.Models.Client.ToDto(client);
            type = "client";
        }
        else if (user is ShopGular.backend.Models.Seller seller)
        {
            typedUserDto = ShopGular.backend.Models.Seller.ToDto(seller);
            type = "seller";
        }
        else
        {
            typedUserDto = Models.User.ToDto(user);
            type = "user";
        }

        return Ok(new { type, user = typedUserDto });
    }
}