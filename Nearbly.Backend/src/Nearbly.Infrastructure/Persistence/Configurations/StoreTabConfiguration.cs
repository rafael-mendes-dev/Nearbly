using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nearbly.Domain.Entities;

namespace Nearbly.Infrastructure.Persistence.Configurations;

public sealed class StoreTabConfiguration : IEntityTypeConfiguration<StoreTab>
{
    public void Configure(EntityTypeBuilder<StoreTab> builder)
    {
        builder.ToTable("store_tabs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ContentType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.StoreId, x.Key }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.IsActive, x.SortOrder });
        builder.HasOne(x => x.Store).WithMany(x => x.Tabs).HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
    }
}
