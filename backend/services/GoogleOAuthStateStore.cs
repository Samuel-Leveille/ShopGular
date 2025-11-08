using Microsoft.Extensions.Caching.Memory;
using ShopGular.Backend.Models.Dtos;

namespace ShopGular.Backend.Services;

public record GoogleOAuthStoredResult(GoogleLoginResponseDto Result, string ReturnUrl);

public interface IGoogleOAuthStateStore
{
    string CreateState(string returnUrl);
    bool TryConsumeState(string state, out string returnUrl);
    void StoreResult(string state, GoogleLoginResponseDto result, string returnUrl);
    bool TryConsumeResult(string state, out GoogleOAuthStoredResult stored);
}

public class GoogleOAuthStateStore : IGoogleOAuthStateStore
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _stateLifetime = TimeSpan.FromMinutes(10);

    public GoogleOAuthStateStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string CreateState(string returnUrl)
    {
        var state = Guid.NewGuid().ToString("N");
        _cache.Set(StateKey(state), returnUrl, _stateLifetime);
        return state;
    }

    public bool TryConsumeState(string state, out string returnUrl)
    {
        if (_cache.TryGetValue<string>(StateKey(state), out var value) && !string.IsNullOrEmpty(value))
        {
            _cache.Remove(StateKey(state));
            returnUrl = value;
            return true;
        }

        returnUrl = "/";
        return false;
    }

    public void StoreResult(string state, GoogleLoginResponseDto result, string returnUrl)
    {
        _cache.Set(ResultKey(state), new GoogleOAuthStoredResult(result, returnUrl), _stateLifetime);
    }

    public bool TryConsumeResult(string state, out GoogleOAuthStoredResult stored)
    {
        if (_cache.TryGetValue<GoogleOAuthStoredResult>(ResultKey(state), out var value) && value != null)
        {
            _cache.Remove(ResultKey(state));
            stored = value;
            return true;
        }

        stored = null!;
        return false;
    }

    private static string StateKey(string state) => $"google-oauth-state:{state}";
    private static string ResultKey(string state) => $"google-oauth-result:{state}";
}

