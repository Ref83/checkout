using Npgsql;

namespace Interview.PaymentGateway.Postgres;

public interface IConnectionFactory
{
    NpgsqlConnection Create();
}