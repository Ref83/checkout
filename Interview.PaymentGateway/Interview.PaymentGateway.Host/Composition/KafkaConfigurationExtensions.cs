using Interview.PaymentGateway.Host.Workers;
using Interview.PaymentGateway.Postgres.Kafka;

namespace Interview.PaymentGateway.Host.Composition;

public static class KafkaConfigurationExtensions
{
    public static IServiceCollection ConfigureKafka(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var brokers = configuration.GetSection("Kafka:Brokers").Get<string>();

        services.AddSingleton<IKafkaProducerFactory<string, string>>(new KafkaProducerFactory<string, string>(brokers));
        
        services.AddHostedService<EventPublisher>();
        
        return services;
    }
}