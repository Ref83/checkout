namespace Interview.Bank.Host.Controllers.Contracts;

public sealed class PaymentRequest
{
    public string PaymentId { get; set; } = null!;
    
    public string CardNumber { get; set; } = null!;
    public string Expiry { get; set; } = null!;
    public string Cvv { get; set; } = null!;
    public string? CardHolder { get; set; }
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    
    public string MerchantId { get; set; } = null!;
}