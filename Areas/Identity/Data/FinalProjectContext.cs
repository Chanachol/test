using FinalProject.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinalProject.Data;

public class FinalProjectContext : IdentityDbContext<FinalProjectUser>
{
    public FinalProjectContext(DbContextOptions<FinalProjectContext> options)
        : base(options)
    {
    }

    public object Emails { get; internal set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new FinalProjectUserEntityConfiguration());
    }
}

public class FinalProjectUserEntityConfiguration: IEntityTypeConfiguration<FinalProjectUser> 
{ 
    public void Configure(EntityTypeBuilder<FinalProjectUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(255);
        builder.Property(u => u.LastName).HasMaxLength(255);
        builder.Property(u => u.MobilePhone).HasMaxLength(255);
    }
}