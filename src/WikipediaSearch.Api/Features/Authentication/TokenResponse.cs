namespace WikipediaSearch.Api.Features.Authentication;

public sealed record TokenResponse(
  string AccessToken,
  DateTimeOffset ExpiresAtUtc,
  string TokenType = "Bearer");