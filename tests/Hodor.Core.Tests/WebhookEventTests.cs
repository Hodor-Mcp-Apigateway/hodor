using System;
using FluentAssertions;
using Hodor.Core.Webhooks;
using Xunit;

namespace Hodor.Core.Tests;

public class WebhookEventTests
{
    [Fact]
    public void WebhookEvent_Record_StoresValues()
    {
        var ts = DateTime.UtcNow;
        var evt = new WebhookEvent("evt-1", "tool.call", ts, new { tool = "mem:get" });

        evt.Id.Should().Be("evt-1");
        evt.Type.Should().Be("tool.call");
        evt.Timestamp.Should().Be(ts);
        evt.Data.Should().NotBeNull();
    }
}
