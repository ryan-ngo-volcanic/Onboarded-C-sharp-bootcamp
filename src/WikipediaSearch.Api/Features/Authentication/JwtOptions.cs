namespace WikipediaSearch.Api.Features.Authentication;

/// <summary>
/// Defines the configuration used to issue and validate authentication tokens.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Identifies the service that issues access tokens.
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Identifies the intended consumer of access tokens.
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    /// Contains the secret key used to sign access tokens.
    /// </summary>
    public required string SigningKey { get; init; }

    /// <summary>
    /// Defines how many minutes an access token remains valid.
    /// </summary>
    public int AccessTokenMinutes { get; init; }

    /// <summary>
    /// Defines how many days a refresh token remains valid.
    /// </summary>
    public int RefreshTokenDays { get; init; }
}