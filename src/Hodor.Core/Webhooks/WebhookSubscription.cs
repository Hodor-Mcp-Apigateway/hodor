namespace Hodor.Core.Webhooks;

/// <summary>
/// Webhook subscription - URL to receive events.
/// </summary>
public record WebhookSubscription(
    string Id,
    string Url,
    string[] Events,
    string? Secret,
    string? Description,
    DateTime CreatedAt
);
