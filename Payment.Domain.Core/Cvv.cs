namespace Payment.Domain.Core;

public readonly record struct Cvv(string Value)
{
    public string Value {get;} = IsValid(Value) ? Value : throw new ArgumentException(nameof(Value));

    private static bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        if (value.Length != 3)
            throw new ArgumentException("Cvv length should be 3", nameof(value));

        foreach (var numberChar in value)
            if (!char.IsDigit(numberChar))
                throw new ArgumentException("Cvv should contain only digits", nameof(value));

        return true;
    }
}