using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WikipediaSearch.Api.Data.Entities;
using WikipediaSearch.Api.Features.Authentication;

namespace WikipediaSearch.Api.Tests;

public class TokenServiceTests
{
  private const string TestIssuer = "test-issuer";
  private const string TestAudience = "test-audience";
  private const int TokenLifetimeMinutes = 1440;

  private static TokenService CreateService()
  {
    var options = Options.Create(new JwtOptions
    {
      Issuer = TestIssuer,
      Audience = TestAudience,
      SigningKey = new string('k', 64),
      AccessTokenMinutes = TokenLifetimeMinutes
    });

    return new TokenService(options);
  }

  private static ApplicationUser CreateUser()
  {
    return new ApplicationUser
    {
      Id = "user-123",
      Email = "user@example.com",
      UserName = "user@example.com"
    };
  }

  private static JsonWebToken ReadToken(string accessToken)
  {
    return new JsonWebTokenHandler()
      .ReadJsonWebToken(accessToken);
  }

  [Fact]
  public void GenerateAccessToken_WithValidUser_ReturnsTokenAndExpiration
 ()
  {
    var service = CreateService();
    var user = CreateUser();
    var beforeGeneration = DateTimeOffset.UtcNow;

    var result = service.GenerateAccessToken(user);

    var afterGeneration = DateTimeOffset.UtcNow;

    Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));

    Assert.InRange(
      result.ExpiresAtUtc,
      beforeGeneration.AddMinutes(TokenLifetimeMinutes),
      afterGeneration.AddMinutes(TokenLifetimeMinutes));

    var token = ReadToken(result.AccessToken);
    var tokenExpiration = new DateTimeOffset(
      token.ValidTo,
      TimeSpan.Zero);

    Assert.Equal(
      result.ExpiresAtUtc.ToUnixTimeSeconds(),
      tokenExpiration.ToUnixTimeSeconds());
  }

  [Fact]
  public void GenerateAccessToken_WithValidUser_ContainsRequiredClaims()
  {
    var service = CreateService();
    var user = CreateUser();

    var result = service.GenerateAccessToken(user);
    var token = ReadToken(result.AccessToken);

    var subject = token.Claims.Single(
      claim => claim.Type == JwtRegisteredClaimNames.Sub);

    var email = token.Claims.Single(
      claim => claim.Type == JwtRegisteredClaimNames.Email);

    var tokenId = token.Claims.Single(
      claim => claim.Type == JwtRegisteredClaimNames.Jti);

    Assert.Equal(user.Id, subject.Value);
    Assert.Equal(user.Email, email.Value);
    Assert.True(Guid.TryParse(tokenId.Value, out _));
  }


  [Fact]
  public void GenerateAccessToken_WithValidUser_UsesConfiguredSecuritySettings()
  {
    var service = CreateService();

    var result = service.GenerateAccessToken(CreateUser());
    var token = ReadToken(result.AccessToken);

    Assert.Equal(TestIssuer, token.Issuer);
    Assert.Contains(TestAudience, token.Audiences);
    Assert.Equal(SecurityAlgorithms.HmacSha256, token.Alg);
  }

  [Fact]
  public void GenerateAccessToken_WithoutEmail_ThrowsInvalidOperationException()
  {
    var service = CreateService();
    var user = new ApplicationUser
    {
      Id = "user-123",
      Email = null
    };

    Assert.Throws<InvalidOperationException>(() =>
      service.GenerateAccessToken(user));
  }

  [Fact]
  public void GenerateAccessToken_WithNullUser_ThrowsArgumentNullException()
  {
    var service = CreateService();

    Assert.Throws<ArgumentNullException>(() =>
      service.GenerateAccessToken(null!));
  }
}