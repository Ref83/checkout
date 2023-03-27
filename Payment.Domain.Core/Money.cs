namespace Payment.Domain.Core;

public readonly record struct Money(decimal Amount, Currency Currency)
{
    public decimal Amount {get;} = Amount < 0 
        ? throw new ArgumentException("Money can't be negative", nameof(Amount)) 
        : Amount;
}