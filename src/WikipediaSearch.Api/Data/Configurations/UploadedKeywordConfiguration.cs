using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WikipediaSearch.Api.Data.Entities;

namespace WikipediaSearch.Api.Data.Configurations;

public class UploadedKeywordConfiguration
    : IEntityTypeConfiguration<UploadedKeyword>
{
    public void Configure(EntityTypeBuilder<UploadedKeyword> builder)
    {
        builder.ToTable("UploadedKeywords");

        builder.HasKey(keyword => keyword.Id);

        builder.Property(keyword => keyword.OriginalKeyword)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(keyword => keyword.NormalizedKeyword)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(keyword => keyword.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(keyword => keyword.AttemptCount)
            .IsRequired();

        builder.Property(keyword => keyword.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasOne(keyword => keyword.UploadBatch)
            .WithMany(batch => batch.Keywords)
            .HasForeignKey(keyword => keyword.UploadBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(keyword => keyword.UploadBatchId);

        builder.HasIndex(keyword => new
        {
            keyword.UploadBatchId,
            keyword.NormalizedKeyword
        })
        .IsUnique();
    }
}