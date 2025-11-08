using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShopGular.Backend.Models.Dtos;

namespace ShopGular.Backend.Services;

public class PendingSignupTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly int _lifetimeMinutes;

    public PendingSignupTokenService(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        _issuer = jwtSection["Issuer"] ?? "ShopGular";
        _audience = jwtSection["Audience"] ?? "ShopGular.GoogleSignup";
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt secret manquant pour la génération du jeton Google.");
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        _lifetimeMinutes = int.TryParse(jwtSection["GoogleSignupPendingMinutes"], out var minutes) ? minutes : 10;
    }

    public string CreateToken(PendingGoogleSignupDto dto)
    {
        var claims = new List<Claim>
        {
            new("purpose", "google-signup"),
            new(JwtRegisteredClaimNames.Sub, dto.Subject ?? dto.Email),
            new(JwtRegisteredClaimNames.Email, dto.Email),
            new("email", dto.Email)
        };

        AddOptionalClaim(claims, "first_name", dto.FirstName);
        AddOptionalClaim(claims, "last_name", dto.LastName);
        AddOptionalClaim(claims, "full_name", dto.FullName);
        AddOptionalClaim(claims, "picture", dto.Picture);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _issuer,
            audience: $"{_audience}:google-signup",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_lifetimeMinutes),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public PendingGoogleSignupDto? Validate(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = $"{_audience}:google-signup",
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt ||
                jwt.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value != "google-signup")
            {
                return null;
            }

            var email = principal.FindFirstValue("email") ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return new PendingGoogleSignupDto(
                email,
                principal.FindFirstValue("first_name"),
                principal.FindFirstValue("last_name"),
                principal.FindFirstValue("full_name"),
                principal.FindFirstValue("picture"),
                principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            );
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private static void AddOptionalClaim(List<Claim> claims, string type, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(type, value));
        }
    }
}

