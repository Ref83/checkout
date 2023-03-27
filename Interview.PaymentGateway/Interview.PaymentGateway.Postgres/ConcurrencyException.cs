namespace Interview.PaymentGateway.Postgres;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message)
    {
    }
}
