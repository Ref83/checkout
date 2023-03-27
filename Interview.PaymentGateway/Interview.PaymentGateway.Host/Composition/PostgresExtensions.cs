using Interview.PaymentGateway.Application.Querying;
using Interview.PaymentGateway.Domain;
using Interview.PaymentGateway.Postgres;
using Interview.PaymentGateway.Postgres.Kafka;

namespace Interview.PaymentGateway.Host.Composition;

public static class PostgresExtensions
{
    private const string ConnectionStringConfigurationSection = "Postgres:ConnectionString";

    public static IServiceCollection ConfigurePostgresServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(ConnectionStringConfigurationSection).Get<string>();
        services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory(connectionString));
        services.AddSingleton<IPaymentRepository, PaymentRepository>();
        services.AddSingleton<IPaymentQueryService, PaymentQueryService>();
        services.AddSingleton<OutboxEventPublisher>();

        return services;
    }
}