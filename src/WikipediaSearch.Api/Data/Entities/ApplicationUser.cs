using Microsoft.AspNetCore.Identity;

namespace WikipediaSearch.Api.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public ICollection<UploadBatch> UploadBatches { get; set; } = new List<UploadBatch>();
}