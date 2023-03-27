using Interview.PaymentExecutor.Application.Events;

namespace Interview.PaymentExecutor.Application;

public interface IPaymentRetryService
{
    Task Send(PaymentCreated message, CancellationToken token);
}