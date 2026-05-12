using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Task1.Framework.Contracts;

namespace Task1.Framework.Tests;

public sealed class ItemsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ItemsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task Post_then_get_by_id_returns_created_item()
    {
        var create = new CreateItemRequest("Прочитать главу про middleware", 5);
        var post = await _client.PostAsJsonAsync("/api/items", create);

        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        Assert.True(post.Headers.TryGetValues("X-Request-Id", out var requestIds));
        Assert.Contains(requestIds, x => !string.IsNullOrWhiteSpace(x));

        var created = await post.Content.ReadFromJsonAsync<ItemDto>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(create.Title, created.Title);
        Assert.Equal(create.Points, created.Points);

        var get = await _client.GetAsync($"/api/items/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var fetched = await get.Content.ReadFromJsonAsync<ItemDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Get_unknown_id_returns_uniform_404_error_with_request_id()
    {
        var response = await _client.GetAsync("/api/items/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("X-Request-Id", out var requestIds));
        var requestIdFromHeader = requestIds.FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(requestIdFromHeader));

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(ErrorCodes.ItemNotFound, error!.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.Equal(requestIdFromHeader, error.RequestId);
    }

    [Fact]
    public async Task Post_invalid_payload_returns_uniform_400_error()
    {
        var response = await _client.PostAsJsonAsync("/api/items", new CreateItemRequest("   ", -1));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(ErrorCodes.ValidationError, error!.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.RequestId));
    }
}
