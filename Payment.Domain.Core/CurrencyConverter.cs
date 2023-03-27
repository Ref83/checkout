namespace Payment.Domain.Core;

public static class CurrencyConverter
{
    public static Currency Convert(string code)
    {
        if (!Enum.TryParse<Currency>(code, true, out var currency))
            throw new ArgumentException("Unknown currency: " + code, nameof(code));

        return currency;
    }
}