using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WikipediaSearch.Api.Data.Entities;

namespace WikipediaSearch.Api.Data.Configurations;

public class WikipediaSearchResultConfiguration
    : IEntityTypeConfiguration<WikipediaSearchResult>
{
    public void Configure(
        EntityTypeBuilder<WikipediaSearchResult> builder)
    {
        builder.ToTable("WikipediaSearchResults", table =>
        {
            table.HasCheckConstraint(
                   "CK_WikipediaSearchResults_TotalResults",
                   "[TotalResults] >= 0");

            table.HasCheckConstraint(
                   "CK_WikipediaSearchResults_ThumbnailCounts",
                   "[ResultsWithThumbnails] >= 0 AND " +
                   "[ResultsWithoutThumbnails] >= 0");

            table.HasCheckConstraint(
                   "CK_WikipediaSearchResults_TotalLinks",
                   "[TotalLinks] >= 0");
        });

        builder.HasKey(result => result.Id);

        builder.Property(result => result.Html)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(result => result.SourceUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(result => result.CapturedAtUtc)
            .IsRequired();

        builder.HasOne(result => result.UploadedKeyword)
            .WithOne(keyword => keyword.SearchResult)
            .HasForeignKey<WikipediaSearchResult>(
                result => result.UploadedKeywordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(result => result.UploadedKeywordId)
            .IsUnique();
    }
}