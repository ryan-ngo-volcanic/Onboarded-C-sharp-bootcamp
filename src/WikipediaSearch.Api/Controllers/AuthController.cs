using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WikipediaSearch.Api.Data.Entities;
using WikipediaSearch.Api.Features.Authentication;

namespace WikipediaSearch.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
  UserManager<ApplicationUser> userManager,
  TokenService tokenService)
  : ControllerBase
{
  [AllowAnonymous]
  [HttpPost("sign-up")]
  public async Task<ActionResult<TokenResponse>> SignUp(
    SignUpRequest request)
  {
    var email = request.Email.Trim();

    var user = new ApplicationUser
    {
      Email = email,
      UserName = email
    };

    var result = await userManager.CreateAsync(
      user,
      request.Password);

    if (!result.Succeeded)
    {
      return BadRequest(new
      {
        Errors = result.Errors
          .Select(error => error.Description)
          .ToArray()
      });
    }

    var token = tokenService.GenerateAccessToken(user);

    return StatusCode(
      StatusCodes.Status201Created,
      ToResponse(token));
  }

  [AllowAnonymous]
  [HttpPost("sign-in")]
  public async Task<ActionResult<TokenResponse>> SignIn(SignInRequest request)
  {
    var email = request.Email.Trim();

    var user = await userManager.FindByEmailAsync(email);

    if (user is null ||
        !await userManager.CheckPasswordAsync(
          user,
          request.Password))
    {
      return Unauthorized(new
      {
        Error = "Invalid email or password."
      });
    }

    var token = tokenService.GenerateAccessToken(user);

    return Ok(ToResponse(token));
  }

  private static TokenResponse ToResponse(
    AccessTokenResult token)
  {
    return new TokenResponse(
      token.AccessToken,
      token.ExpiresAtUtc);
  }
}
