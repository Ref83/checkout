namespace Interview.PaymentExecutor.Host.Services.Messages;

public sealed class PaymentCompleted
{
    public string PaymentId { get; set; } = null!;
}