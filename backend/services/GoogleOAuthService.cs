using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using ShopGular.Backend.Models.Dtos;

namespace ShopGular.Backend.Services;

public interface IGoogleOAuthService
{
    string CreateAuthorizationUrl(string returnUrl);
    Task<GoogleOAuthCallbackOutcome> HandleCallbackAsync(string state, string code, CancellationToken cancellationToken);
    string CreateSuccessRedirect(string state);
    string CreateErrorRedirect(string error, string? state, string? message = null);
}

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly GoogleAuthService _googleAuthService;
    private readonly IGoogleLoginCoordinator _googleLoginCoordinator;
    private readonly IGoogleOAuthStateStore _stateStore;

    public GoogleOAuthService(
        IConfiguration configuration,
        HttpClient httpClient,
        GoogleAuthService googleAuthService,
        IGoogleLoginCoordinator googleLoginCoordinator,
        IGoogleOAuthStateStore stateStore)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _googleAuthService = googleAuthService;
        _googleLoginCoordinator = googleLoginCoordinator;
        _stateStore = stateStore;
    }

    private string ClientId => _configuration["Authentication:Google:ClientId"] ?? string.Empty;
    private string ClientSecret => _configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
    private string RedirectUri => _configuration["Authentication:Google:RedirectUri"] ?? string.Empty;
    private string PostLoginRedirect => _configuration["Authentication:Google:PostLoginRedirect"] ?? "http://localhost:4200/google-callback";

    public string CreateAuthorizationUrl(string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(RedirectUri))
        {
            throw new InvalidOperationException("La configuration Google OAuth est incomplète.");
        }

        var state = _stateStore.CreateState(returnUrl);
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = ClientId,
            ["redirect_uri"] = RedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["include_granted_scopes"] = "true",
            ["state"] = state
        };

        var queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}"));
        return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
    }

    public async Task<GoogleOAuthCallbackOutcome> HandleCallbackAsync(string state, string code, CancellationToken cancellationToken)
    {
        if (!_stateStore.TryConsumeState(state, out var returnUrl))
        {
            throw new InvalidOperationException("Session OAuth Google inconnue ou expirée.");
        }

        var tokenResponse = await ExchangeCodeAsync(code, cancellationToken);
        if (string.IsNullOrWhiteSpace(tokenResponse.IdToken))
        {
            throw new InvalidOperationException("Google n'a pas retourné d'ID token.");
        }

        var payload = await _googleAuthService.ValidateIdTokenAsync(tokenResponse.IdToken, cancellationToken);
        if (payload == null)
        {
            throw new InvalidOperationException("Impossible de valider le token Google retourné.");
        }

        var result = await _googleLoginCoordinator.HandlePayloadAsync(payload);
        return new GoogleOAuthCallbackOutcome(result, returnUrl);
    }

    public string CreateSuccessRedirect(string state)
        => $"{PostLoginRedirect}?state={Uri.EscapeDataString(state)}";

    public string CreateErrorRedirect(string error, string? state, string? message = null)
    {
        var baseUrl = $"{PostLoginRedirect}?error={Uri.EscapeDataString(error)}";
        if (!string.IsNullOrWhiteSpace(state))
        {
            baseUrl += $"&state={Uri.EscapeDataString(state)}";
        }
        if (!string.IsNullOrWhiteSpace(message))
        {
            baseUrl += $"&message={Uri.EscapeDataString(message)}";
        }
        return baseUrl;
    }

    private async Task<GoogleTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        var content = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["redirect_uri"] = RedirectUri,
            ["grant_type"] = "authorization_code"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(content)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var token = await JsonSerializer.DeserializeAsync<GoogleTokenResponse>(stream, cancellationToken: cancellationToken);
        if (token == null)
        {
            throw new InvalidOperationException("Réponse inattendue de Google lors de l'échange du code.");
        }

        return token;
    }
}

public record GoogleOAuthCallbackOutcome(GoogleLoginResponseDto Result, string ReturnUrl);

public sealed class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;
}

