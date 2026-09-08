namespace WikipediaSearch.Api.Data.Entities;

public class UploadBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public required string OriginalFileName { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;

    public ICollection<UploadedKeyword> Keywords { get; set; } =
        new List<UploadedKeyword>();
}