using Payment.Domain.Core;

namespace Interview.PaymentGateway.Domain;

public sealed record Payment
{
    public Payment(
        PaymentId id,
        CardInformation card,
        Money amount,
        MerchantId merchantId,
        PaymentStatus status = PaymentStatus.Processing,
        string? reason = null,
        uint version = 0)
    {
        Id = id;
        Card = card;
        Amount = amount;
        MerchantId = merchantId;
        Status = status;
        Reason = reason;
        Version = version;
    }
    
    public PaymentId Id { get; }
    public CardInformation Card { get; } 
    public Money Amount { get; } 
    public MerchantId MerchantId { get; } 
    public PaymentStatus Status { get; private set; }
    public string? Reason { get; private set; }
    
    public uint Version { get; set; }

    public void Complete() 
        => Status = PaymentStatus.Completed;

    public void Reject(string reason)
    {
        // It is possible when executor executes payment and place it to payment_completed topic, 
        // but doesn't commit offset in payments_created topic because of some failure
        // After that executor retry payment but can't do it because bank api is over.
        // So executor publish payment_rejected message
        // To avoid this scenario let's consider Completed status as terminal status 
        if (Status == PaymentStatus.Completed)
            return;

        Status = PaymentStatus.Error;
        Reason = reason;
    }
}