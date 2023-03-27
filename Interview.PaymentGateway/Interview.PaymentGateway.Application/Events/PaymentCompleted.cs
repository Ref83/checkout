namespace Interview.PaymentGateway.Application.Events;

public sealed class PaymentCompleted
{
    public string PaymentId { get; set; } = null!;
}