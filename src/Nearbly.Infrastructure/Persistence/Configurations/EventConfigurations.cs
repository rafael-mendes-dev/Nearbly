using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nearbly.Domain.Entities;

namespace Nearbly.Infrastructure.Persistence.Configurations;

public sealed class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> builder)
    {
        builder.ToTable("page_views");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.StoreId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.StoreId, x.Source, x.OccurredAtUtc });
        builder.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LinkClickConfiguration : IEntityTypeConfiguration<LinkClick>
{
    public void Configure(EntityTypeBuilder<LinkClick> builder)
    {
        builder.ToTable("link_clicks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.StoreId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.LinkId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.StoreId, x.Source, x.OccurredAtUtc });
        builder.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Link).WithMany().HasForeignKey(x => x.LinkId).OnDelete(DeleteBehavior.Restrict);
    }
}
