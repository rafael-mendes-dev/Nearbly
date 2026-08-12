using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Nearbly.Application.Features.Auth;
using Nearbly.Application.Features.Public;
using Nearbly.Infrastructure.Persistence;
using Nearbly.Domain.Entities;

namespace Nearbly.IntegrationTests;

public sealed class PublicFlowTests(NearblyApiFixture fixture) : IClassFixture<NearblyApiFixture>
{
    [Fact]
    public async Task FullFlow_AuthenticatesManagesStoreAndTracksPublicEvents()
    {
        using var client = fixture.Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var unauthorized = await client.GetAsync("/api/admin/stores");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal("application/problem+json", unauthorized.Content.Headers.ContentType?.MediaType);

        var swagger = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
        var swaggerJson = await swagger.Content.ReadAsStringAsync();
        Assert.Contains("Bearer", swaggerJson, StringComparison.Ordinal);
        using (var swaggerDocument = JsonDocument.Parse(swaggerJson))
        {
            var paths = swaggerDocument.RootElement.GetProperty("paths");
            Assert.True(paths.GetProperty("/api/admin/stores").GetProperty("get").GetProperty("security").GetArrayLength() > 0);
            Assert.False(paths.GetProperty("/api/public/stores/{slug}").GetProperty("get").TryGetProperty("security", out _));
            Assert.Contains("Get a public store page", swaggerJson, StringComparison.Ordinal);
        }

        var invalidLogin = await client.PostAsJsonAsync("/api/admin/auth/login", new { email = "not-an-email", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidLogin.StatusCode);
        Assert.Equal("application/problem+json", invalidLogin.Content.Headers.ContentType?.MediaType);

        var invalidCredentials = await client.PostAsJsonAsync("/api/admin/auth/login", new LoginRequest("admin@test.local", "wrong-password"));
        Assert.Equal(HttpStatusCode.Unauthorized, invalidCredentials.StatusCode);
        Assert.Equal("application/problem+json", invalidCredentials.Content.Headers.ContentType?.MediaType);

        var missingPublicStore = await client.GetAsync("/api/public/stores/does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, missingPublicStore.StatusCode);
        Assert.Equal("application/problem+json", missingPublicStore.Content.Headers.ContentType?.MediaType);

        var malformedJson = await client.PostAsync("/api/public/stores/does-not-exist/views", new StringContent("{", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, malformedJson.StatusCode);
        Assert.Equal("application/problem+json", malformedJson.Content.Headers.ContentType?.MediaType);

        var login = await client.PostAsJsonAsync("/api/admin/auth/login", new LoginRequest("admin@test.local", "ChangeMe123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var createStore = await client.PostAsJsonAsync("/api/admin/stores", new { name = "Café Central", slug = "Cafe Central", description = "Centro", primaryColor = "#112233" });
        Assert.Equal(HttpStatusCode.Created, createStore.StatusCode);
        using var storeJson = JsonDocument.Parse(await createStore.Content.ReadAsStringAsync());
        var storeId = storeJson.RootElement.GetProperty("id").GetGuid();

        var duplicateStore = await client.PostAsJsonAsync("/api/admin/stores", new { name = "Outra Loja", slug = "cafe-central" });
        Assert.Equal(HttpStatusCode.Conflict, duplicateStore.StatusCode);

        var createTab = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs", new { key = "menu", name = "Menu", sortOrder = 0 });
        Assert.Equal(HttpStatusCode.Created, createTab.StatusCode);
        using var tabJson = JsonDocument.Parse(await createTab.Content.ReadAsStringAsync());
        var tabId = tabJson.RootElement.GetProperty("id").GetGuid();

        var createOtherStore = await client.PostAsJsonAsync("/api/admin/stores", new { name = "Outra Loja", slug = "outra-loja" });
        Assert.Equal(HttpStatusCode.Created, createOtherStore.StatusCode);
        using var otherStoreJson = JsonDocument.Parse(await createOtherStore.Content.ReadAsStringAsync());
        var otherStoreId = otherStoreJson.RootElement.GetProperty("id").GetGuid();
        var createOtherTab = await client.PostAsJsonAsync($"/api/admin/stores/{otherStoreId}/tabs", new { key = "other", name = "Other" });
        Assert.Equal(HttpStatusCode.Created, createOtherTab.StatusCode);
        using var otherTabJson = JsonDocument.Parse(await createOtherTab.Content.ReadAsStringAsync());
        var otherTabId = otherTabJson.RootElement.GetProperty("id").GetGuid();

        var invalidAssociation = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/links", new { type = "website", label = "Invalid", url = "https://example.com/invalid", storeTabId = otherTabId });
        Assert.Equal(HttpStatusCode.Conflict, invalidAssociation.StatusCode);

        var createLink = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/links", new { type = "website", label = "Site", icon = "globe", url = "https://example.com/site", sortOrder = 0, storeTabId = tabId });
        Assert.Equal(HttpStatusCode.Created, createLink.StatusCode);
        using var linkJson = JsonDocument.Parse(await createLink.Content.ReadAsStringAsync());
        var linkId = linkJson.RootElement.GetProperty("id").GetGuid();

        var rootLink = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/links", new { type = "instagram", label = "Instagram", url = "https://example.com/instagram", sortOrder = 1 });
        Assert.Equal(HttpStatusCode.Created, rootLink.StatusCode);
        using var rootLinkJson = JsonDocument.Parse(await rootLink.Content.ReadAsStringAsync());
        var rootLinkId = rootLinkJson.RootElement.GetProperty("id").GetGuid();

        client.DefaultRequestHeaders.Authorization = null;
        var publicStore = await client.GetFromJsonAsync<PublicStoreResponse>("/api/public/stores/cafe-central");
        Assert.NotNull(publicStore);
        Assert.Single(publicStore!.Tabs);
        Assert.Equal($"/r/{linkId}", publicStore.Tabs[0].Links[0].Href);
        Assert.Single(publicStore.Links);
        Assert.DoesNotContain("example.com", JsonSerializer.Serialize(publicStore));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var productTab = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs", new { key = "products", name = "Produtos", contentType = "products", sortOrder = 1 });
        Assert.Equal(HttpStatusCode.Created, productTab.StatusCode);
        var productTabId = (await productTab.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var markdownTab = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs", new { key = "about", name = "Sobre", contentType = "markdown", sortOrder = 2 });
        Assert.Equal(HttpStatusCode.Created, markdownTab.StatusCode);
        var markdownTabId = (await markdownTab.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var galleryTab = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs", new { key = "photos", name = "Fotos", contentType = "gallery", sortOrder = 3 });
        Assert.Equal(HttpStatusCode.Created, galleryTab.StatusCode);
        var galleryTabId = (await galleryTab.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        using var invalidUploadContent = new MultipartFormDataContent();
        invalidUploadContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("not-an-image")), "file", "fake.png");
        var invalidUpload = await client.PostAsync($"/api/admin/stores/{storeId}/media", invalidUploadContent);
        Assert.Equal(HttpStatusCode.BadRequest, invalidUpload.StatusCode);
        using var oversizedUploadContent = new MultipartFormDataContent();
        var oversizedImage = new ByteArrayContent(new byte[(5 * 1024 * 1024) + 1]);
        oversizedImage.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        oversizedUploadContent.Add(oversizedImage, "file", "large.png");
        var oversizedUpload = await client.PostAsync($"/api/admin/stores/{storeId}/media", oversizedUploadContent);
        Assert.Equal(HttpStatusCode.BadRequest, oversizedUpload.StatusCode);

        using var uploadContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        uploadContent.Add(imageContent, "file", "pixel.png");
        var upload = await client.PostAsync($"/api/admin/stores/{storeId}/media", uploadContent);
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var media = await upload.Content.ReadFromJsonAsync<JsonElement>();
        var mediaId = media.GetProperty("id").GetGuid();
        Assert.Equal("image/webp", media.GetProperty("mimeType").GetString());
        var servedMedia = await client.GetAsync($"/media/{mediaId}");
        Assert.Equal(HttpStatusCode.OK, servedMedia.StatusCode);
        Assert.Equal("image/webp", servedMedia.Content.Headers.ContentType?.MediaType);

        var product = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs/{productTabId}/products", new { name = "Café coado", description = "250 ml", mediaAssetId = mediaId, price = 8.5m, isAvailable = true, sortOrder = 0 });
        Assert.Equal(HttpStatusCode.Created, product.StatusCode);
        var wrongType = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs/{productTabId}/markdown-blocks", new { title = "Inválido", markdown = "texto" });
        Assert.Equal(HttpStatusCode.Conflict, wrongType.StatusCode);
        var markdown = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs/{markdownTabId}/markdown-blocks", new { title = "Horários", markdown = "## Semana\n\n*Aberto*", sortOrder = 0 });
        Assert.Equal(HttpStatusCode.Created, markdown.StatusCode);
        var gallery = await client.PostAsJsonAsync($"/api/admin/stores/{storeId}/tabs/{galleryTabId}/gallery-items", new { mediaAssetId = mediaId, altText = "Café coado", caption = "Nosso café", sortOrder = 0 });
        Assert.Equal(HttpStatusCode.Created, gallery.StatusCode);
        var changeTypeWithContent = await client.PutAsJsonAsync($"/api/admin/stores/{storeId}/tabs/{productTabId}", new { key = "products", name = "Produtos", contentType = "markdown", sortOrder = 1, isActive = true });
        Assert.Equal(HttpStatusCode.Conflict, changeTypeWithContent.StatusCode);
        var referencedMedia = await client.DeleteAsync($"/api/admin/stores/{storeId}/media/{mediaId}");
        Assert.Equal(HttpStatusCode.Conflict, referencedMedia.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var publicWithContent = await client.GetFromJsonAsync<PublicStoreResponse>("/api/public/stores/cafe-central");
        Assert.Equal("products", publicWithContent!.Tabs.Single(tab => tab.Id == productTabId).ContentType);
        Assert.Single(publicWithContent.Tabs.Single(tab => tab.Id == productTabId).Products);
        Assert.Single(publicWithContent.Tabs.Single(tab => tab.Id == markdownTabId).MarkdownBlocks);
        Assert.Single(publicWithContent.Tabs.Single(tab => tab.Id == galleryTabId).GalleryItems);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var view = await client.PostAsJsonAsync("/api/public/stores/cafe-central/views", new RegisterPageViewRequest(TrafficSource.Nfc));
        Assert.Equal(HttpStatusCode.NoContent, view.StatusCode);
        var secondView = await client.PostAsJsonAsync("/api/public/stores/cafe-central/views", new RegisterPageViewRequest(TrafficSource.QrCode));
        Assert.Equal(HttpStatusCode.NoContent, secondView.StatusCode);

        var redirect = await client.GetAsync($"/r/{linkId}?src=qr_code");
        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal("https://example.com/site", redirect.Headers.Location?.ToString());
        var secondClick = await client.GetAsync($"/r/{linkId}?src=direct");
        Assert.Equal(HttpStatusCode.Redirect, secondClick.StatusCode);
        var rootRedirect = await client.GetAsync($"/r/{rootLinkId}?src=direct");
        Assert.Equal(HttpStatusCode.Redirect, rootRedirect.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var analytics = await client.GetAsync($"/api/admin/stores/{storeId}/analytics");
        Assert.Equal(HttpStatusCode.OK, analytics.StatusCode);
        var analyticsJson = JsonDocument.Parse(await analytics.Content.ReadAsStringAsync());
        Assert.Equal(2, analyticsJson.RootElement.GetProperty("views").GetInt64());
        Assert.Equal(3, analyticsJson.RootElement.GetProperty("clicks").GetInt64());
        Assert.Equal(150, analyticsJson.RootElement.GetProperty("ctr").GetDecimal());
        Assert.Equal(1, analyticsJson.RootElement.GetProperty("sources").GetProperty("Nfc").GetInt64());
        Assert.Equal(1, analyticsJson.RootElement.GetProperty("sources").GetProperty("QrCode").GetInt64());
        var topLinks = analyticsJson.RootElement.GetProperty("topLinks").EnumerateArray().ToList();
        Assert.Equal(2, topLinks.Count);
        Assert.Equal("Site", topLinks[0].GetProperty("label").GetString());
        Assert.Equal(2, topLinks[0].GetProperty("clicks").GetInt64());
        Assert.Single(analyticsJson.RootElement.GetProperty("viewsByDay").EnumerateArray());

        var analyticsByDay = await client.GetAsync($"/api/admin/stores/{storeId}/analytics?from={DateTime.UtcNow:yyyy-MM-dd}&to={DateTime.UtcNow:yyyy-MM-dd}");
        Assert.Equal(HttpStatusCode.OK, analyticsByDay.StatusCode);
        var emptyHistoricalAnalytics = await client.GetAsync($"/api/admin/stores/{storeId}/analytics?from=2000-01-01&to=2000-01-02");
        Assert.Equal(HttpStatusCode.OK, emptyHistoricalAnalytics.StatusCode);
        using var emptyAnalyticsJson = JsonDocument.Parse(await emptyHistoricalAnalytics.Content.ReadAsStringAsync());
        Assert.Equal(0, emptyAnalyticsJson.RootElement.GetProperty("views").GetInt64());
        Assert.Equal(0, emptyAnalyticsJson.RootElement.GetProperty("clicks").GetInt64());
        Assert.Equal(0, emptyAnalyticsJson.RootElement.GetProperty("ctr").GetDecimal());
        var invalidPeriod = await client.GetAsync($"/api/admin/stores/{storeId}/analytics?from=2026-12-31&to=2026-01-01");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPeriod.StatusCode);
        Assert.Equal("application/problem+json", invalidPeriod.Content.Headers.ContentType?.MediaType);

        var deactivateTab = await client.DeleteAsync($"/api/admin/stores/{storeId}/tabs/{tabId}");
        Assert.Equal(HttpStatusCode.NoContent, deactivateTab.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        var publicWithoutTab = await client.GetFromJsonAsync<PublicStoreResponse>("/api/public/stores/cafe-central");
        Assert.DoesNotContain(publicWithoutTab!.Tabs, tab => tab.Id == tabId);
        var redirectFromInactiveTab = await client.GetAsync($"/r/{linkId}?src=direct");
        Assert.Equal(HttpStatusCode.Redirect, redirectFromInactiveTab.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var inactiveTabs = await client.GetFromJsonAsync<List<JsonElement>>($"/api/admin/stores/{storeId}/tabs");
        Assert.Contains(inactiveTabs!, tab => !tab.GetProperty("isActive").GetBoolean());
        var reactivateTab = await client.PutAsJsonAsync($"/api/admin/stores/{storeId}/tabs/{tabId}", new { key = "menu", name = "Menu", sortOrder = 0, isActive = true });
        Assert.Equal(HttpStatusCode.OK, reactivateTab.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        var publicAfterReactivation = await client.GetFromJsonAsync<PublicStoreResponse>("/api/public/stores/cafe-central");
        Assert.Contains(publicAfterReactivation!.Tabs, tab => tab.Id == tabId);

        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NearblyDbContext>();
        Assert.Equal(2, db.PageViews.Count());
        Assert.Equal(4, db.LinkClicks.Count());
    }
}
