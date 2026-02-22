namespace Hodor.Core.Webhooks;

/// <summary>
/// Dispatches events to registered webhook URLs.
/// </summary>
public interface IWebhookDispatcher
{
    Task DispatchAsync(WebhookEvent evt, CancellationToken cancellationToken = default);
}

/// <summary>
/// Webhook event payload.
/// </summary>
public record WebhookEvent(
    string Id,
    string Type,
    DateTime Timestamp,
    object Data
);

/// <summary>
/// Supported event types.
/// </summary>
public static class WebhookEventTypes
{
    public const string ToolCall = "tool.call";
    public const string ServerStarted = "server.started";
    public const string ServerStopped = "server.stopped";

    public static readonly string[] All = [ToolCall, ServerStarted, ServerStopped];
}
