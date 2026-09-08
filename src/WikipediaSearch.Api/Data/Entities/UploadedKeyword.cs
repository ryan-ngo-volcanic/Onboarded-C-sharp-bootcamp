namespace WikipediaSearch.Api.Data.Entities;

public class UploadedKeyword
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UploadBatchId { get; set; }

    public UploadBatch UploadBatch { get; set; } = null!;

    public required string OriginalKeyword { get; set; }

    public required string NormalizedKeyword { get; set; }

    public KeywordProcessingStatus Status { get; set; } =
        KeywordProcessingStatus.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public string? ErrorMessage { get; set; }

    public WikipediaSearchResult? SearchResult { get; set; }
}
