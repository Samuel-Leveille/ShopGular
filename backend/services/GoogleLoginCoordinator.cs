using Google.Apis.Auth;
using ShopGular.Backend.Models.Dtos;

namespace ShopGular.Backend.Services;

public interface IGoogleLoginCoordinator
{
    Task<GoogleLoginResponseDto> HandlePayloadAsync(GoogleJsonWebSignature.Payload payload);
}

public class GoogleLoginCoordinator : IGoogleLoginCoordinator
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;
    private readonly IGooglePendingSignupStore _pendingSignupStore;

    public GoogleLoginCoordinator(
        UserService userService,
        TokenService tokenService,
        IGooglePendingSignupStore pendingSignupStore)
    {
        _userService = userService;
        _tokenService = tokenService;
        _pendingSignupStore = pendingSignupStore;
    }

    public async Task<GoogleLoginResponseDto> HandlePayloadAsync(GoogleJsonWebSignature.Payload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new InvalidOperationException("Le payload Google ne contient pas d'adresse courriel.");
        }

        var existingUser = await _userService.GetUserByEmailAsync(payload.Email);
        if (existingUser != null)
        {
            var (type, dto) = await _userService.MapUserToDtoAsync(existingUser);
            var (accessToken, accessExpires) = _tokenService.CreateAccessToken(existingUser);
            var (refreshToken, refreshExpires) = await _tokenService.CreateAndStoreRefreshTokenAsync(existingUser.Id);

            return new GoogleLoginResponseDto(
                RequiresCompletion: false,
                PendingToken: null,
                Profile: null,
                LoginPayload: new GoogleLoginPayloadDto(
                    type,
                    dto,
                    accessToken,
                    accessExpires,
                    refreshToken,
                    refreshExpires
                )
            );
        }

        var pendingToken = _pendingSignupStore.Store(new PendingGoogleSignupDto(
            Email: payload.Email,
            FirstName: payload.GivenName,
            LastName: payload.FamilyName,
            FullName: payload.Name,
            Picture: payload.Picture,
            Subject: payload.Subject
        ));

        var profile = new GoogleLoginProfileDto(
            payload.Email,
            payload.GivenName,
            payload.FamilyName,
            payload.Name,
            payload.Picture
        );

        return new GoogleLoginResponseDto(
            RequiresCompletion: true,
            PendingToken: pendingToken,
            Profile: profile,
            LoginPayload: null
        );
    }
}

