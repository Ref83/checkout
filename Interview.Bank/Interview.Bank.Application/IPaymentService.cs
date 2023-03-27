using Payment.Domain.Core;

namespace Interview.Bank.Application;

public interface IPaymentService
{
    Task Pay(PaymentInformation paymentInformation, CancellationToken token);
}