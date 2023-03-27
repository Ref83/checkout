namespace Payment.Domain.Core;

public readonly record struct Expiry(string Value)
{
    public string Value {get;} = IsValid(Value) ? Value : throw new ArgumentException(nameof(Value));

    private static bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        if (value.Length != 5)
            throw new ArgumentException("Expiry length should be 5", nameof(value));

        if (value[2] != '/' || !TryGetMonth(out var month) || !TryGetYear(out var year))
            throw new ArgumentException("Expiry should satisfy the mask mm/yy", nameof(value));
        
        if (month is <= 0 or > 12)
            throw new ArgumentException("Month should be in range [1 .. 12]", nameof(value));
        
        return true;

        bool TryGetMonth(out int month)
        {
            month = 0;
            if (!char.IsDigit(value[0]) || !char.IsDigit(value[1]))
                return false;

            month = ToInt(value[0]) * 10 + ToInt(value[1]);
            return true;
        }
        
        bool TryGetYear(out int year)
        {
            year = 0;
            if (!char.IsDigit(value[3]) || !char.IsDigit(value[4]))
                return false;

            year = ToInt(value[3]) * 10 + ToInt(value[4]);
            return true;
        }

        int ToInt(char digitChar)
            => digitChar - '0';
    }
}