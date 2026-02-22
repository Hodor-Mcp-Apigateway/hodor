using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hodor.Core.Webhooks;

namespace Hodor.Host.Webhooks;

/// <summary>
/// Webhook registry + dispatcher. Best-practice single-endpoint design.
/// </summary>
public class WebhookService : IWebhookDispatcher
{
    private readonly ILogger<WebhookService> _logger;
    private readonly HttpClient _http;
    private readonly ConcurrentDictionary<string, WebhookSubscription> _subscriptions = new();
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public WebhookService(ILogger<WebhookService> logger, IHttpClientFactory httpFactory)
    {
        _logger = logger;
        _http = httpFactory.CreateClient("webhook");
        _http.Timeout = TimeSpan.FromSeconds(10);
    }

    public WebhookSubscription Register(string url, string[]? events = null, string? secret = null, string? description = null)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.Scheme.StartsWith("http"))
            throw new ArgumentException("Invalid webhook URL", nameof(url));

        var sub = new WebhookSubscription(
            Id: Guid.NewGuid().ToString("N")[..16],
            Url: url.TrimEnd('/'),
            Events: events ?? WebhookEventTypes.All,
            Secret: secret,
            Description: description,
            CreatedAt: DateTime.UtcNow
        );
        _subscriptions[sub.Id] = sub;
        _logger.LogInformation("Webhook registered: {Id} -> {Url}", sub.Id, sub.Url);
        return sub;
    }

    public bool Unregister(string id)
    {
        return _subscriptions.TryRemove(id, out _);
    }

    public IReadOnlyList<WebhookSubscription> List() => _subscriptions.Values.ToList();

    public async Task DispatchAsync(WebhookEvent evt, CancellationToken cancellationToken = default)
    {
        var subs = _subscriptions.Values
            .Where(s => s.Events.Contains(evt.Type, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (subs.Count == 0) return;

        var payload = JsonSerializer.Serialize(new
        {
            evt.Id,
            evt.Type,
            evt.Timestamp,
            evt.Data
        }, JsonOpts);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        foreach (var sub in subs)
        {
            _ = FireAndForgetAsync(sub, payload, payloadBytes, evt.Type, evt.Id, cancellationToken);
        }
    }

    private async Task FireAndForgetAsync(WebhookSubscription sub, string payload, byte[] payloadBytes, string eventType, string deliveryId, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, sub.Url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("X-Hodor-Event", eventType);
            req.Headers.Add("X-Hodor-Delivery", deliveryId);
            req.Headers.Add("X-Idempotency-Key", deliveryId);
            if (!string.IsNullOrEmpty(sub.Secret))
            {
                var sig = ComputeHmacSha256(payloadBytes, sub.Secret);
                req.Headers.Add("X-Hodor-Signature", $"sha256={sig}");
            }

            var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                _logger.LogWarning("Webhook {Url} returned {Status}", sub.Url, res.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook delivery failed: {Url}", sub.Url);
        }
    }

    private static string ComputeHmacSha256(byte[] payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(payload);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
