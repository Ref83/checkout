namespace Payment.Domain.Core;

public readonly record struct PaymentId(string Value)
{
    public string Value {get;} = IsValid(Value) ? Value : throw new ArgumentException(nameof(Value));

    private static bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        if (value.Length > 64)
            throw new ArgumentException("PaymentId length should less then 64", nameof(value));

        return true;
    }
}