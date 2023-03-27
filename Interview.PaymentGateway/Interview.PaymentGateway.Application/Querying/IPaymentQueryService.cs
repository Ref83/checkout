using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application.Querying;

public interface IPaymentQueryService
{
    Task<PaymentInformation> Get(PaymentId paymentId, string merchantId, CancellationToken token);
}