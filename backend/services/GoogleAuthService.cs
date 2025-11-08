using Google.Apis.Auth;

namespace ShopGular.Backend.Services;

public class GoogleAuthService
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public GoogleAuthService(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? GetClientId() => _configuration["Authentication:Google:ClientId"];

    public async Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var clientId = GetClientId();
        if (string.IsNullOrWhiteSpace(idToken) || string.IsNullOrWhiteSpace(clientId))
        {
            return null;
        }

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { clientId }
        };

        try
        {
            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}

