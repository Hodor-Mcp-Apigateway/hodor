using System;
using FluentAssertions;
using Hodor.Core.Webhooks;
using Xunit;

namespace Hodor.Core.Tests;

public class WebhookSubscriptionTests
{
    [Fact]
    public void WebhookSubscription_Record_StoresValues()
    {
        var created = DateTime.UtcNow;
        var sub = new WebhookSubscription(
            Id: "abc123",
            Url: "https://example.com/hook",
            Events: [WebhookEventTypes.ToolCall],
            Secret: "secret",
            Description: "Test",
            CreatedAt: created
        );

        sub.Id.Should().Be("abc123");
        sub.Url.Should().Be("https://example.com/hook");
        sub.Events.Should().Contain(WebhookEventTypes.ToolCall);
        sub.Secret.Should().Be("secret");
        sub.Description.Should().Be("Test");
        sub.CreatedAt.Should().Be(created);
    }
}
