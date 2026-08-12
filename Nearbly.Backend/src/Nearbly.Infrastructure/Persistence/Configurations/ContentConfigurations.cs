using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nearbly.Domain.Entities;

namespace Nearbly.Infrastructure.Persistence.Configurations;

public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.StoreId, x.IsActive, x.CreatedAtUtc });
        builder.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2_000);
        builder.Property(x => x.Price).HasPrecision(12, 2);
        builder.HasIndex(x => new { x.StoreTabId, x.IsActive, x.SortOrder });
        builder.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StoreTab).WithMany(x => x.Products).HasForeignKey(x => x.StoreTabId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MediaAsset).WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class MarkdownBlockConfiguration : IEntityTypeConfiguration<MarkdownBlock>
{
    public void Configure(EntityTypeBuilder<MarkdownBlock> builder)
    {
        builder.ToTable("markdown_blocks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160);
        builder.Property(x => x.Markdown).HasMaxLength(20_000).IsRequired();
        builder.HasIndex(x => new { x.StoreTabId, x.IsActive, x.SortOrder });
        builder.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StoreTab).WithMany(x => x.MarkdownBlocks).HasForeignKey(x => x.StoreTabId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class GalleryItemConfiguration : IEntityTypeConfiguration<GalleryItem>
{
    public void Configure(EntityTypeBuilder<GalleryItem> builder)
    {
        builder.ToTable("gallery_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AltText).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Caption).HasMaxLength(500);
        builder.HasIndex(x => new { x.StoreTabId, x.IsActive, x.SortOrder });
        builder.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StoreTab).WithMany(x => x.GalleryItems).HasForeignKey(x => x.StoreTabId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MediaAsset).WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
    }
}
