namespace Papel.Integration.Application.Wallet.EventHandlers;

using Papel.Integration.Common.Extensions;

public sealed class SendMoneyEventBusHandler : INotificationHandler<SendMoneyEvent>
{
    private readonly ILogger<SendMoneyEventBusHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public SendMoneyEventBusHandler(ILogger<SendMoneyEventBusHandler> logger,
        IPublishEndpoint publishEndpoint, IMapper mapper)
    {
        _mapper = mapper.ThrowIfNull();
        _logger = logger.ThrowIfNull();
        _publishEndpoint = publishEndpoint.ThrowIfNull();
    }

    public async Task Handle(SendMoneyEvent notification, CancellationToken cancellationToken)
    {
        _logger.RaiseIntegrationEvent(notification.GetType().Name);

        var createEvent = await notification
            .BuildAdapter(_mapper.Config)
            .AdaptToTypeAsync<ToDoItemCreatedIntegrationEvent>()
            .ConfigureAwait(false);

        await _publishEndpoint.Publish(createEvent, cancellationToken)
            .ConfigureAwait(false);
    }
}
