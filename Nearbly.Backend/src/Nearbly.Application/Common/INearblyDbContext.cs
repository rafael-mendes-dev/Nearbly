using Microsoft.EntityFrameworkCore;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Common;

public interface INearblyDbContext
{
    DbSet<Store> Stores { get; }
    DbSet<StoreTab> StoreTabs { get; }
    DbSet<Link> Links { get; }
    DbSet<Product> Products { get; }
    DbSet<MarkdownBlock> MarkdownBlocks { get; }
    DbSet<GalleryItem> GalleryItems { get; }
    DbSet<MediaAsset> MediaAssets { get; }
    DbSet<PageView> PageViews { get; }
    DbSet<LinkClick> LinkClicks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
