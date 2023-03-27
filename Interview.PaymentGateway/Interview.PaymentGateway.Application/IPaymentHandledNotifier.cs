using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application;

public interface IPaymentHandledNotifier
{
    void NotifyCompleted(PaymentId paymentId);
    
    void NotifyRejected(PaymentId paymentId, string reason);
}