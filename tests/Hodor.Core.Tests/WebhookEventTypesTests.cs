using FluentAssertions;
using Hodor.Core.Webhooks;
using Xunit;

namespace Hodor.Core.Tests;

public class WebhookEventTypesTests
{
    [Fact]
    public void All_ContainsExpectedEvents()
    {
        WebhookEventTypes.All.Should().Contain(WebhookEventTypes.ToolCall);
        WebhookEventTypes.All.Should().Contain(WebhookEventTypes.ServerStarted);
        WebhookEventTypes.All.Should().Contain(WebhookEventTypes.ServerStopped);
        WebhookEventTypes.All.Should().HaveCount(3);
    }

    [Fact]
    public void ToolCall_HasExpectedValue()
    {
        WebhookEventTypes.ToolCall.Should().Be("tool.call");
    }

    [Fact]
    public void ServerStarted_HasExpectedValue()
    {
        WebhookEventTypes.ServerStarted.Should().Be("server.started");
    }

    [Fact]
    public void ServerStopped_HasExpectedValue()
    {
        WebhookEventTypes.ServerStopped.Should().Be("server.stopped");
    }
}
