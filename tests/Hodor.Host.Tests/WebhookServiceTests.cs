using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Hodor.Core.Webhooks;
using Hodor.Host.Webhooks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hodor.Host.Tests;

public class WebhookServiceTests
{
    private readonly Mock<ILogger<WebhookService>> _loggerMock = new();

    private WebhookService CreateService(HttpMessageHandler? handler = null)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("webhook"))
            .Returns(handler != null ? new HttpClient(handler) : new HttpClient());
        return new WebhookService(_loggerMock.Object, factory.Object);
    }

    [Fact]
    public void Register_ValidUrl_ReturnsSubscription()
    {
        var service = CreateService();
        var sub = service.Register("https://example.com/hook");

        sub.Should().NotBeNull();
        sub.Id.Should().NotBeNullOrEmpty();
        sub.Url.Should().Be("https://example.com/hook");
        sub.Events.Should().BeEquivalentTo(WebhookEventTypes.All);
        sub.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Register_WithEvents_FiltersEvents()
    {
        var service = CreateService();
        var sub = service.Register("https://example.com/hook", events: [WebhookEventTypes.ToolCall]);

        sub.Events.Should().ContainSingle(WebhookEventTypes.ToolCall);
    }

    [Fact]
    public void Register_InvalidUrl_Throws()
    {
        var service = CreateService();

        var act = () => service.Register("not-a-url");
        act.Should().Throw<ArgumentException>().WithParameterName("url");
    }

    [Fact]
    public void Register_NonHttpUrl_Throws()
    {
        var service = CreateService();

        var act = () => service.Register("ftp://example.com/hook");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Unregister_ExistingId_ReturnsTrue()
    {
        var service = CreateService();
        var sub = service.Register("https://example.com/hook");

        var result = service.Unregister(sub.Id);

        result.Should().BeTrue();
        service.List().Should().BeEmpty();
    }

    [Fact]
    public void Unregister_NonExistingId_ReturnsFalse()
    {
        var service = CreateService();

        var result = service.Unregister("nonexistent");

        result.Should().BeFalse();
    }

    [Fact]
    public void List_ReturnsAllSubscriptions()
    {
        var service = CreateService();
        service.Register("https://a.com");
        service.Register("https://b.com");

        var list = service.List();

        list.Should().HaveCount(2);
    }

    [Fact]
    public void Register_TrimsTrailingSlash()
    {
        var service = CreateService();
        var sub = service.Register("https://example.com/hook/");

        sub.Url.Should().Be("https://example.com/hook");
    }

    [Fact]
    public async Task DispatchAsync_NoSubscriptions_DoesNotThrow()
    {
        var service = CreateService();
        var evt = new WebhookEvent("1", WebhookEventTypes.ToolCall, DateTime.UtcNow, new { });

        var act = () => service.DispatchAsync(evt);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DispatchAsync_WithSubscription_SendsRequest()
    {
        var received = false;
        var handler = new MockHttpMessageHandler((req, _) =>
        {
            received = req.Method == HttpMethod.Post &&
                      req.RequestUri?.ToString().Contains("example.com") == true &&
                      req.Headers.Contains("X-Hodor-Event");
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var service = CreateService(handler);
        service.Register("https://example.com/hook", events: [WebhookEventTypes.ToolCall]);

        await service.DispatchAsync(new WebhookEvent("1", WebhookEventTypes.ToolCall, DateTime.UtcNow, new { data = 1 }));

        await Task.Delay(100);
        received.Should().BeTrue();
    }

    [Fact]
    public async Task DispatchAsync_WithSecret_AddsSignatureHeader()
    {
        string? signature = null;
        var handler = new MockHttpMessageHandler((req, _) =>
        {
            if (req.Headers.TryGetValues("X-Hodor-Signature", out var values))
                signature = values.FirstOrDefault();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var service = CreateService(handler);
        service.Register("https://example.com/hook", events: WebhookEventTypes.All, secret: "my-secret");

        await service.DispatchAsync(new WebhookEvent("1", WebhookEventTypes.ToolCall, DateTime.UtcNow, new { }));

        await Task.Delay(100);
        signature.Should().NotBeNullOrEmpty();
        signature.Should().StartWith("sha256=");
    }

    [Fact]
    public async Task DispatchAsync_FiltersByEventType()
    {
        var callCount = 0;
        var handler = new MockHttpMessageHandler((_, _) =>
        {
            Interlocked.Increment(ref callCount);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var service = CreateService(handler);
        service.Register("https://example.com/hook", events: [WebhookEventTypes.ServerStarted]);

        await service.DispatchAsync(new WebhookEvent("1", WebhookEventTypes.ToolCall, DateTime.UtcNow, new { }));
        await Task.Delay(50);
        var countAfterToolCall = callCount;

        await service.DispatchAsync(new WebhookEvent("2", WebhookEventTypes.ServerStarted, DateTime.UtcNow, new { }));
        await Task.Delay(100);

        countAfterToolCall.Should().Be(0);
        callCount.Should().Be(1);
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request, cancellationToken));
        }
    }
}
