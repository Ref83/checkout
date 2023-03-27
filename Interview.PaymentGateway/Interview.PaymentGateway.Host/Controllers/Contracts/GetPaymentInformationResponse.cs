namespace Interview.PaymentGateway.Host.Controllers.Contracts;

public sealed class GetPaymentInformationResponse
{
    public string PaymentId { get; set; } = null!;

    public string MaskedCardNumber { get; set; } = null!;
    public string Expiry { get; set; } = null!;
    public string? CardHolder { get; set; }
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;

    public DateTime Date { get; set; }
    
    public string Status { get; set; } = null!;
    public string? Reason { get; set; }

}