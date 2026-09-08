using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WikipediaSearch.Api.Data.Entities;

namespace WikipediaSearch.Api.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<UploadBatch> UploadBatches => Set<UploadBatch>();

    public DbSet<UploadedKeyword> UploadedKeywords => Set<UploadedKeyword>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }

    public DbSet<WikipediaSearchResult> WikipediaSearchResults => Set<WikipediaSearchResult>();
}