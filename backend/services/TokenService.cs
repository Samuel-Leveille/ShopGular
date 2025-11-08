using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using ShopGular.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopGular.Backend.Services;
public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;

    public TokenService(IConfiguration configuration, AppDbContext db)
    {
        _configuration = configuration;
        _db = db;
    }

    public (string accessToken, DateTime accessExpires) CreateAccessToken(User user)
    {
        var jwt = _configuration.GetSection("Jwt");
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name)
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwt["Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }

    public async Task<(string refreshToken, DateTime refreshExpires)> CreateAndStoreRefreshTokenAsync(long userId)
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[64];
        rng.GetBytes(bytes);
        var token = Convert.ToBase64String(bytes);
        var days = int.Parse(_configuration["Jwt:RefreshTokenDays"]!);
        var expires = DateTime.UtcNow.AddDays(days);
        var entity = new RefreshToken { UserId = userId, Token = token, ExpiresAtUtc = expires };
        _db.Set<RefreshToken>().Add(entity);
        await _db.SaveChangesAsync();
        return (token, expires);
    }
}

public class RefreshTokenService
{
    private readonly AppDbContext _db;
    public RefreshTokenService(AppDbContext db) { _db = db; }

    public async Task<RefreshToken?> GetValidAsync(string token)
    {
        var rt = await _db.Set<RefreshToken>().FirstOrDefaultAsync(r => r.Token == token);
        if (rt == null) return null;
        if (rt.RevokedAtUtc != null) return null;
        if (rt.ExpiresAtUtc <= DateTime.UtcNow) return null;
        return rt;
    }

    public async Task RevokeAsync(string token)
    {
        var rt = await _db.Set<RefreshToken>().FirstOrDefaultAsync(r => r.Token == token);
        if (rt != null)
        {
            rt.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}

