using Interview.PaymentGateway.Application.Events;
using Interview.PaymentGateway.Domain;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentHandledAwaiter _paymentHandledAwaiter;
    private readonly string _paymentCreatedTopic;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IPaymentHandledAwaiter paymentHandledAwaiter,
        string paymentCreatedTopic)
    {
        _paymentRepository = paymentRepository;
        _paymentHandledAwaiter = paymentHandledAwaiter;
        _paymentCreatedTopic = paymentCreatedTopic;
    }

    public Task<PaymentId> GenerateId(MerchantId merchantId, CancellationToken token)
    {
        // Just mock PaymentId generation
        // In production we can use some external distributed id generator
        return Task.FromResult(new PaymentId(Guid.NewGuid().ToString()));
    }
    
    public async Task Pay(
        PaymentId paymentId,
        CardInformation cardInformation,
        Money amount,
        MerchantId merchantId, 
        CancellationToken token)
    {
        var payment = new Domain.Payment(paymentId, cardInformation, amount, merchantId);
        var added = await _paymentRepository.TryAdd(payment, GetPaymentCreatedEvent(payment), token);

        if (added)
            await _paymentHandledAwaiter.Await(paymentId, token);
    }

    private Event GetPaymentCreatedEvent(Domain.Payment payment)
    {
        return new Event(
            new PaymentCreated
            {
                PaymentId = payment.Id.Value,
                
                CardNumber = payment.Card.CardNumber.Value,
                Expiry = payment.Card.Expiry.Value,
                Cvv = payment.Card.Cvv.Value,
                CardHolder = payment.Card.CardHolder,
                
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency.ToString(),
                
                MerchantId = payment.MerchantId.Value
            },
            _paymentCreatedTopic);
    }
}