using Nearbly.Domain.Entities;
using Nearbly.Domain.Services;

namespace Nearbly.UnitTests;

public sealed class EntityInvariantTests
{
    [Fact]
    public void Link_RejectsNegativeSortOrder()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Link(Guid.NewGuid(), "website", "Site", null, "https://example.com", -1));
    }

    [Fact]
    public void Link_RejectsInvalidUrl()
    {
        Assert.Throws<ArgumentException>(() => new Link(Guid.NewGuid(), "website", "Site", null, "javascript:alert(1)"));
    }

    [Fact]
    public void Store_CanBeDeactivatedAndReactivatedWithoutChangingChildren()
    {
        var store = new Store("Store", "Minha Loja");
        store.Deactivate();
        Assert.False(store.IsActive);
        store.Activate();
        Assert.True(store.IsActive);
    }

    [Fact]
    public void Tab_DefaultsToLinksAndCanChangeTypeBeforeReceivingContent()
    {
        var tab = new StoreTab(Guid.NewGuid(), "menu", "Menu");

        Assert.Equal(ContentType.Links, tab.ContentType);
        tab.ChangeContentType(ContentType.Products, hasContent: false);

        Assert.Equal(ContentType.Products, tab.ContentType);
    }

    [Fact]
    public void Tab_CannotChangeTypeAfterContentWasAdded()
    {
        var tab = new StoreTab(Guid.NewGuid(), "menu", "Menu");
        tab.Links.Add(new Link(tab.StoreId, "website", "Site", null, "https://example.com", storeTabId: tab.Id));

        Assert.Throws<InvalidOperationException>(() => tab.ChangeContentType(ContentType.Markdown, hasContent: true));
    }

    [Fact]
    public void Product_RequiresNonNegativePriceAndImage()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(Guid.NewGuid(), Guid.NewGuid(), "Café", null, Guid.NewGuid(), -1, true));
        Assert.Throws<ArgumentException>(() => new Product(Guid.NewGuid(), Guid.NewGuid(), "Café", null, Guid.Empty, 10, true));
    }

    [Fact]
    public void GalleryItem_RequiresAlternativeText()
    {
        Assert.Throws<ArgumentException>(() => new GalleryItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "", null));
    }
}
