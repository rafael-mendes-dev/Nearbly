using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nearbly.Domain.Entities;

namespace Nearbly.Infrastructure.Persistence.Configurations;

public sealed class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable("links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(120);
        builder.Property(x => x.Url).HasMaxLength(2_048).IsRequired();
        builder.HasIndex(x => new { x.StoreId, x.IsActive, x.SortOrder });
        builder.HasIndex(x => new { x.StoreTabId, x.SortOrder });
        builder.HasOne(x => x.Store).WithMany(x => x.Links).HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StoreTab).WithMany(x => x.Links).HasForeignKey(x => x.StoreTabId).OnDelete(DeleteBehavior.Restrict);
    }
}
