using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application;

public interface IPaymentService
{
    Task<PaymentId> GenerateId(MerchantId merchantId, CancellationToken token);

    Task Pay(
        PaymentId paymentId,
        CardInformation cardInformation,
        Money amount,
        MerchantId merchantId, 
        CancellationToken token);
}