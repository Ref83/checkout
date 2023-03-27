using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application;

public interface IPaymentHandledAwaiter
{
    Task Await(PaymentId paymentId, CancellationToken token);
}