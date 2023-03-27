using Payment.Domain.Core;

namespace Interview.PaymentExecutor.Application;

public interface IPaymentCompletedNotifier
{
    Task Notify(PaymentId paymentId, CancellationToken token);
}