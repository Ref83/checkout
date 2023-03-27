namespace Payment.Domain.Core;

public readonly record struct CardInformation(
    CardNumber CardNumber,
    Expiry Expiry,
    Cvv Cvv,
    string? CardHolder);
