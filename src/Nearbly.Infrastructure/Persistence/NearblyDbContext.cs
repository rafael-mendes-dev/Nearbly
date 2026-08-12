using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Infrastructure.Persistence;

public sealed class NearblyDbContext(DbContextOptions<NearblyDbContext> options) : IdentityUserContext<IdentityUser>(options), INearblyDbContext
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreTab> StoreTabs => Set<StoreTab>();
    public DbSet<Link> Links => Set<Link>();
    public DbSet<PageView> PageViews => Set<PageView>();
    public DbSet<LinkClick> LinkClicks => Set<LinkClick>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<IdentityUser>().ToTable("asp_net_users");
        builder.Entity<IdentityUserClaim<string>>().ToTable("asp_net_user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("asp_net_user_logins");
        builder.Entity<IdentityUserToken<string>>().ToTable("asp_net_user_tokens");
        builder.ApplyConfigurationsFromAssembly(typeof(NearblyDbContext).Assembly);
    }
}
