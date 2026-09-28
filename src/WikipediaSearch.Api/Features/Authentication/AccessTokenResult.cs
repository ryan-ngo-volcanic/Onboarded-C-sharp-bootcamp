namespace WikipediaSearch.Api.Features.Authentication;

public sealed record AccessTokenResult(
  string AccessToken,
  DateTimeOffset ExpiresAtUtc);