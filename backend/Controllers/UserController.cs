using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Models;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;
using IGoogleLoginCoordinator = ShopGular.Backend.Services.IGoogleLoginCoordinator;
using IGoogleOAuthService = ShopGular.Backend.Services.IGoogleOAuthService;
using IGoogleOAuthStateStore = ShopGular.Backend.Services.IGoogleOAuthStateStore;
using IGooglePendingSignupStore = ShopGular.Backend.Services.IGooglePendingSignupStore;
using System.Security.Claims;
using System.Linq;

namespace ShopGular.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;
    private readonly RefreshTokenService _refreshService;
    private readonly GoogleAuthService _googleAuthService;
    private readonly IGoogleLoginCoordinator _googleLoginCoordinator;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IGoogleOAuthStateStore _googleOAuthStateStore;
    private readonly IGooglePendingSignupStore _pendingSignupStore;

    public UserController(
        UserService userService,
        TokenService tokenService,
        RefreshTokenService refreshService,
        GoogleAuthService googleAuthService,
        IGooglePendingSignupStore pendingSignupStore,
        IGoogleLoginCoordinator googleLoginCoordinator,
        IGoogleOAuthService googleOAuthService,
        IGoogleOAuthStateStore googleOAuthStateStore)
    {
        _userService = userService;
        _tokenService = tokenService;
        _refreshService = refreshService;
        _googleAuthService = googleAuthService;
        _pendingSignupStore = pendingSignupStore;
        _googleLoginCoordinator = googleLoginCoordinator;
        _googleOAuthService = googleOAuthService;
        _googleOAuthStateStore = googleOAuthStateStore;
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

        var payload = await BuildLoginPayloadAsync(user);
        return Ok(payload);
    }

    [HttpPost("google/login")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.IdToken))
        {
            return BadRequest(new { message = "Le jeton Google est requis." });
        }

        var payload = await _googleAuthService.ValidateIdTokenAsync(request.IdToken, cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload?.Email))
        {
            return Unauthorized(new { message = "Impossible de valider l'identité Google." });
        }

        var response = await _googleLoginCoordinator.HandlePayloadAsync(payload);
        return Ok(response);
    }

    [HttpPost("google/complete-signup")]
    public async Task<IActionResult> CompleteGoogleSignup(CompleteGoogleSignupRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.PendingToken))
        {
            return BadRequest(new { message = "Le jeton d'inscription Google est requis." });
        }

        if (!_pendingSignupStore.TryGet(request.PendingToken, out var pending))
        {
            return Unauthorized(new { message = "Le jeton d'inscription Google est invalide ou expiré." });
        }

        var existingUser = await _userService.GetUserByEmailAsync(pending.Email);
        if (existingUser != null)
        {
            var existingResponse = await BuildLoginPayloadAsync(existingUser);
            return Ok(existingResponse);
        }

        var accountType = request.AccountType?.Trim().ToLowerInvariant();
        User createdUser;
        switch (accountType)
        {
            case "client":
            case "customer":
                var (firstName, lastName) = ResolveNames(pending, request.FirstName, request.LastName);
                createdUser = await _userService.CreateClientFromGoogleAsync(pending.Email, firstName, lastName);
                break;
            case "seller":
                var sellerName = (request.SellerName ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(sellerName))
                {
                    return BadRequest(new { message = "Le nom de l'entreprise est requis pour un compte vendeur." });
                }
                createdUser = await _userService.CreateSellerFromGoogleAsync(pending.Email, sellerName);
                break;
            default:
                return BadRequest(new { message = "Type de compte invalide. Utilisez 'client' ou 'seller'." });
        }

        var response = await BuildLoginPayloadAsync(createdUser);
        _pendingSignupStore.Remove(request.PendingToken);
        return Ok(response);
    }

    [HttpGet("google/oauth/start")]
    public IActionResult StartGoogleOAuth([FromQuery] string? returnUrl)
    {
        var url = _googleOAuthService.CreateAuthorizationUrl(returnUrl ?? "/");
        return Ok(new { authorizationUrl = url });
    }

    [HttpGet("google/oauth/callback")]
    public async Task<IActionResult> GoogleOAuthCallback([FromQuery] string? state, [FromQuery] string? code, [FromQuery] string? error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            return Redirect(_googleOAuthService.CreateErrorRedirect(error!, state));
        }

        if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(code))
        {
            return Redirect(_googleOAuthService.CreateErrorRedirect("invalid_request", state));
        }

        try
        {
            var outcome = await _googleOAuthService.HandleCallbackAsync(state!, code!, cancellationToken);
            _googleOAuthStateStore.StoreResult(state!, outcome.Result, outcome.ReturnUrl);
            return Redirect(_googleOAuthService.CreateSuccessRedirect(state!));
        }
        catch (Exception ex)
        {
            return Redirect(_googleOAuthService.CreateErrorRedirect("callback_failed", state, ex.Message));
        }
    }

    [HttpGet("google/oauth/result/{state}")]
    public IActionResult GetGoogleOAuthResult(string state)
    {
        if (!_googleOAuthStateStore.TryConsumeResult(state, out var stored))
        {
            return NotFound(new { message = "Session Google expirée. Veuillez réessayer." });
        }

        return Ok(new { returnUrl = stored.ReturnUrl, result = stored.Result });
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

        var (type, dto) = await _userService.MapUserToDtoAsync(user);
        return Ok(new { type, user = dto });
    }

    private async Task<GoogleLoginPayloadDto> BuildLoginPayloadAsync(User user)
    {
        var (access, accessExp) = _tokenService.CreateAccessToken(user);
        var (refresh, refreshExp) = await _tokenService.CreateAndStoreRefreshTokenAsync(user.Id);
        var (type, dto) = await _userService.MapUserToDtoAsync(user);

        return new GoogleLoginPayloadDto(
            type,
            dto,
            access,
            accessExp,
            refresh,
            refreshExp
        );
    }

    private static (string firstName, string lastName) ResolveNames(PendingGoogleSignupDto pending, string? requestedFirst, string? requestedLast)
    {
        static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        var desiredFirst = Normalize(requestedFirst);
        var desiredLast = Normalize(requestedLast);

        var fallbackFirst = Normalize(pending.FirstName);
        var fallbackLast = Normalize(pending.LastName);

        if (!string.IsNullOrWhiteSpace(pending.FullName))
        {
            var parts = pending.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fallbackFirst == null && parts.Length > 0)
            {
                fallbackFirst = parts[0];
            }
            if (fallbackLast == null && parts.Length > 1)
            {
                fallbackLast = string.Join(" ", parts.Skip(1));
            }
        }

        fallbackFirst ??= pending.Email?.Split('@').FirstOrDefault() ?? "Utilisateur";
        fallbackLast ??= fallbackFirst;

        var firstName = desiredFirst ?? fallbackFirst ?? "Utilisateur";
        var lastName = desiredLast ?? fallbackLast ?? firstName;

        return (firstName, lastName);
    }
}