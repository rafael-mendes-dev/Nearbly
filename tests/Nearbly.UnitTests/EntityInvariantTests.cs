using Nearbly.Domain.Entities;

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
}
