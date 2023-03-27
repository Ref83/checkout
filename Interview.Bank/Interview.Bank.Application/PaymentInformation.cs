using Payment.Domain.Core;

namespace Interview.Bank.Application;

public readonly record struct PaymentInformation(
    PaymentId PaymentId,
    CardInformation CardInformation,
    Money Amount,
    MerchantId MerchantId);