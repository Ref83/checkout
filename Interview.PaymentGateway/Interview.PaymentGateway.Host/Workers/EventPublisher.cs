using Interview.PaymentGateway.Postgres.Kafka;

namespace Interview.PaymentGateway.Host.Workers;

public sealed class EventPublisher : RetryableBackgroundService
{
    private readonly OutboxEventPublisher _outboxEventPublisher;

    public EventPublisher(
        OutboxEventPublisher outboxEventPublisher,
        ILogger<EventPublisher> logger)
        : base(logger) 
        => _outboxEventPublisher = outboxEventPublisher;

    protected override async Task OnExecuteAsync(CancellationToken token)
    {
        await _outboxEventPublisher.Publish(token);
    }
}