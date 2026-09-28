using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WikipediaSearch.Api.Data.Entities;

namespace WikipediaSearch.Api.Features.Authentication;

public sealed class TokenService(
  IOptions<JwtOptions> options
)
{
  private readonly JwtOptions _options = options.Value;
  private readonly JsonWebTokenHandler _tokenHandler = new();

  public AccessTokenResult GenerateAccessToken(
  ApplicationUser user)
  {
    ArgumentNullException.ThrowIfNull(user);

    if (string.IsNullOrWhiteSpace(user.Id) ||
        string.IsNullOrWhiteSpace(user.Email))
    {
      throw new InvalidOperationException(
        "The user must have an ID and email.");
    }

    var issuedAt = DateTimeOffset.UtcNow;
    var expiresAt = issuedAt.AddMinutes(
      _options.AccessTokenMinutes);

    var descriptor = new SecurityTokenDescriptor
    {
      Issuer = _options.Issuer,
      Audience = _options.Audience,
      IssuedAt = issuedAt.UtcDateTime,
      NotBefore = issuedAt.UtcDateTime,
      Expires = expiresAt.UtcDateTime,
      Claims = new Dictionary<string, object>
      {
        [JwtRegisteredClaimNames.Sub] = user.Id,
        [JwtRegisteredClaimNames.Email] = user.Email,
        [JwtRegisteredClaimNames.Jti] =
          Guid.NewGuid().ToString()
      },
      SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(_options.SigningKey)),
        SecurityAlgorithms.HmacSha256)
    };

    return new AccessTokenResult(
      _tokenHandler.CreateToken(descriptor),
      expiresAt);
  }

}