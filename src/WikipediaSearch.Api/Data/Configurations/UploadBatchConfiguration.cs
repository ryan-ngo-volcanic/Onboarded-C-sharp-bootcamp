using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WikipediaSearch.Api.Data.Entities;

namespace WikipediaSearch.Api.Data.Configurations;

public class UploadBatchConfiguration
    : IEntityTypeConfiguration<UploadBatch>
{
    public void Configure(EntityTypeBuilder<UploadBatch> builder)
    {
        builder.ToTable("UploadBatches");

        builder.HasKey(batch => batch.Id);

        builder.Property(batch => batch.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(batch => batch.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(batch => batch.User)
            .WithMany(user => user.UploadBatches)
            .HasForeignKey(batch => batch.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(batch => new
        {
            batch.UserId,
            batch.CreatedAtUtc
        });
    }
}
