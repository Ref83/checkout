namespace Interview.PaymentGateway.Application.Events;

public sealed class PaymentRejected
{
    public string PaymentId { get; set; } = null!;
    
    public string Reason { get; set; } = null!;
}