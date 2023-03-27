using Npgsql;

namespace Interview.PaymentGateway.Postgres;

public sealed class ConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public ConnectionFactory(string connectionString)
        => _connectionString = connectionString;

    public NpgsqlConnection Create()
        => new(_connectionString);
}