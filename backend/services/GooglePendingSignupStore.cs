using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ShopGular.Backend.Models.Dtos;

namespace ShopGular.Backend.Services;

public interface IGooglePendingSignupStore
{
    string Store(PendingGoogleSignupDto dto);
    bool TryGet(string token, out PendingGoogleSignupDto dto);
    void Remove(string token);
}

public class GooglePendingSignupStore : IGooglePendingSignupStore
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _lifetime;

    public GooglePendingSignupStore(IMemoryCache cache, IConfiguration configuration)
    {
        _cache = cache;
        var minutes = int.TryParse(configuration["Jwt:GoogleSignupPendingMinutes"], out var value) ? value : 10;
        _lifetime = TimeSpan.FromMinutes(minutes);
    }

    public string Store(PendingGoogleSignupDto dto)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _cache.Set(Key(token), dto, _lifetime);
        return token;
    }

    public bool TryGet(string token, out PendingGoogleSignupDto dto)
    {
        if (_cache.TryGetValue<PendingGoogleSignupDto>(Key(token), out var stored) && stored != null)
        {
            dto = stored;
            return true;
        }

        dto = null!;
        return false;
    }

    public void Remove(string token)
    {
        _cache.Remove(Key(token));
    }

    private static string Key(string token) => $"google-pending-signup:{token}";
}

