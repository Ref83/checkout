using Payment.Domain.Core;

namespace Interview.PaymentGateway.Domain;

public interface IPaymentRepository
{
    Task<Payment> Get(PaymentId id, CancellationToken token);
    Task<bool> TryAdd(Payment payment, Event @event, CancellationToken token);
    Task Update(Payment payment, CancellationToken token = default);
}