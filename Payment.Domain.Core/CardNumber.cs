namespace Payment.Domain.Core;

public readonly record struct CardNumber(string Value)
{
    public string Value {get;} = IsValid(Value) ? Value : throw new ArgumentException(nameof(Value));

    public string Mask() 
        => $"*{Value[^4..]}";
    private static bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        if (value.Length != 16)
            throw new ArgumentException("Card number length should be 16", nameof(value));

        foreach (var numberChar in value)
            if (!char.IsDigit(numberChar))
                throw new ArgumentException("Card number should contain only digits", nameof(value));

        return true;
    }
}