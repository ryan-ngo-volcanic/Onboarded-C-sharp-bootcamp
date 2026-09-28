using System.ComponentModel.DataAnnotations;

namespace WikipediaSearch.Api.Features.Authentication;

public sealed class SignInRequest
{
  [Required]
  [EmailAddress]
  public required string Email { get; init; }

  [Required]
  public required string Password { get; init; }
}