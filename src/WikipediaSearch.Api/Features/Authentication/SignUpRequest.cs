using System.ComponentModel.DataAnnotations;

namespace WikipediaSearch.Api.Features.Authentication;

public sealed class SignUpRequest
{
  [Required]
  [EmailAddress]
  public required string Email { get; init; }

  [Required]
  public required string Password { get; init; }
}