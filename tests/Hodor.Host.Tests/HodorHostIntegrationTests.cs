using System.Net;
using System.Net.Http.Json;
using Hodor.Core.Webhooks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Hodor.Host.Tests;

public class HodorHostIntegrationTests : IClassFixture<WebApplicationFactory<Hodor.Host.Webhooks.WebhookService>>
{
    private readonly HttpClient _client;

    public HodorHostIntegrationTests(WebApplicationFactory<Hodor.Host.Webhooks.WebhookService> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        }).CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<HealthResponse>();
        json.Should().NotBeNull();
        json!.Status.Should().Be("healthy");
    }

    [Fact]
    public async Task WebhooksEvents_ReturnsEventTypes()
    {
        var response = await _client.GetAsync("/webhooks/events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<WebhookEventsResponse>();
        json.Should().NotBeNull();
        json!.Events.Should().Contain(WebhookEventTypes.ToolCall);
        json.Events.Should().Contain(WebhookEventTypes.ServerStarted);
    }

    [Fact]
    public async Task Webhooks_Register_ValidUrl_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/webhooks", new
        {
            url = "https://example.com/hook",
            events = new[] { "tool.call" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var sub = await response.Content.ReadFromJsonAsync<WebhookSubscription>();
        sub.Should().NotBeNull();
        sub!.Url.Should().Be("https://example.com/hook");
    }

    [Fact]
    public async Task Webhooks_Register_InvalidUrl_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/webhooks", new { url = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApiTools_ReturnsTools()
    {
        var response = await _client.GetAsync("/api/tools");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<ToolsResponse>();
        json.Should().NotBeNull();
        json!.Tools.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfigClaude_ReturnsText()
    {
        var response = await _client.GetAsync("/config/claude");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("claude mcp add");
        text.Should().Contain("/sse");
    }

    [Fact]
    public async Task ConfigCursor_ReturnsJson()
    {
        var response = await _client.GetAsync("/config/cursor");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<CursorConfigResponse>();
        json.Should().NotBeNull();
        json!.McpServers.Should().NotBeNull();
        json.McpServers.Hodor.Should().NotBeNull();
        json.McpServers.Hodor.Url.Should().Contain("/sse");
    }

    private record HealthResponse(string Status);
    private record WebhookEventsResponse(string[] Events);
    private record ToolsResponse(object[] Tools);
    private record CursorConfigResponse(McpServersConfig McpServers);
    private record McpServersConfig(HodorConfig Hodor);
    private record HodorConfig(string Url);
}

