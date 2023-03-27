using Payment.Domain.Core;

namespace Interview.PaymentExecutor.Application;

public interface IBankService
{
    Task Pay(
        PaymentId paymentId,
        CardInformation cardInformation,
        Money amount,
        MerchantId merchantId, 
        CancellationToken token);
}