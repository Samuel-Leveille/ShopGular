using System.Text.Json.Serialization;

namespace ShopGular.Backend.Models.Dtos;

public record GoogleLoginRequest(string IdToken);

public record GoogleLoginProfileDto(string Email, string? FirstName, string? LastName, string? FullName, string? Picture);

public record GoogleLoginPayloadDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("user")] object User,
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("accessTokenExpiresAtUtc")] DateTime AccessTokenExpiresAtUtc,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("refreshTokenExpiresAtUtc")] DateTime RefreshTokenExpiresAtUtc
);

public record GoogleLoginResponseDto(
    bool RequiresCompletion,
    string? PendingToken,
    GoogleLoginProfileDto? Profile,
    GoogleLoginPayloadDto? LoginPayload
);

public record PendingGoogleSignupDto(
    string Email,
    string? FirstName,
    string? LastName,
    string? FullName,
    string? Picture,
    string? Subject
);

public record CompleteGoogleSignupRequest(
    string PendingToken,
    string AccountType,
    string? FirstName,
    string? LastName,
    string? SellerName
);

