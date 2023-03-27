using Interview.PaymentGateway.Domain;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application.Querying;

public sealed record PaymentInformation(
    PaymentId PaymentId,
    CardNumber CardNumber,
    Expiry Expiry,
    string? CardHolder,
    Money Amount,
    DateTime Date,
    PaymentStatus Status,
    string? Reason);