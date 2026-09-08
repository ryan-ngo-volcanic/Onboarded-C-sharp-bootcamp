namespace WikipediaSearch.Api.Data.Entities;

public class WikipediaSearchResult
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UploadedKeywordId { get; set; }

    public UploadedKeyword UploadedKeyword { get; set; } = null!;

    public long TotalResults { get; set; }

    public int ResultsWithThumbnails { get; set; }

    public int ResultsWithoutThumbnails { get; set; }

    public int TotalLinks { get; set; }

    public required string Html { get; set; }

    public required string SourceUrl { get; set; }

    public DateTimeOffset CapturedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;
}