using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using ShopGular.Backend.Services;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;

    public AuthController(UserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync("Google");
        if (!result.Succeeded)
            return BadRequest("Échec d'authentification Google.");

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email non récupéré depuis Google.");

        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return BadRequest("Utilisateur inconnu. Veuillez vous inscrire d'abord.");
        }

        // Génère un JWT pour ton API
        var (accessToken, accessExp) = _tokenService.CreateAccessToken(user);
        var (refreshToken, refreshExp) = await _tokenService.CreateAndStoreRefreshTokenAsync(user.Id);

        // Redirige le frontend avec les tokens dans l'URL (ou via cookie si tu préfères)
        var redirectUrl = $"http://localhost:4200/login-success?accessToken={accessToken}&refreshToken={refreshToken}";
        return Redirect(redirectUrl);
    }
}