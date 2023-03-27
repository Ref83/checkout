namespace Interview.PaymentExecutor.Host.Services.Messages;

public class PaymentRejected
{
    public string PaymentId { get; set; } = null!;
    
    public string Reason { get; set; } = null!;
}