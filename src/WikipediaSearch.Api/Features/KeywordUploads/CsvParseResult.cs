namespace WikipediaSearch.Api.Features.KeywordUploads;

public class CsvParseResult
{
  public CsvParseResult(
      IReadOnlyList<string> keywords,
      IReadOnlyList<string> errors)
  {
    Keywords = keywords;
    Errors = errors;
  }

  public IReadOnlyList<string> Keywords { get; }

  public IReadOnlyList<string> Errors { get; }

  public bool IsValid => Errors.Count == 0;
}
