using Eventso.Subscription.Hosting;
using Eventso.Subscription.Kafka;
using Eventso.Subscription.SpanJson;
using Interview.PaymentExecutor.Application.Events;
using Interview.PaymentExecutor.Application.Handlers;
using Interview.PaymentExecutor.Host.Infrastructure.Kafka;

namespace Interview.PaymentExecutor.Host.Composition.Subscriptions;

public static class KafkaConfigurationExtensions
{
    private const string ConsumerGroupId = "payment_executor";
    private const string PaymentCreatedTopicConfigurationSection = "PaymentCreatedConsumer";
    private const string PaymentFailedTopicConfigurationSection = "PaymentFailedConsumer";

    public static IServiceCollection ConfigureKafka(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var brokers = configuration
            .GetSection("Kafka:Brokers")
            .Get<string>();

        services.AddSingleton<IKafkaProducerFactory<string, string>>(new KafkaProducerFactory<string, string>(brokers));
        
        services
            .AddSubscriptions(
                (s, _) => s
                    .Add(
                        ReadConsumerSettingsFromConfig(PaymentCreatedTopicConfigurationSection),
                        new JsonMessageDeserializer<PaymentCreated>())
                    .Add(
                        ReadConsumerSettingsFromConfig(PaymentFailedTopicConfigurationSection),
                        new JsonMessageDeserializer<PaymentFailed>()),
                handlersSelector: selector =>
                    selector.FromAssembliesOf(typeof(PaymentsHandler)));

        return services;
        
        ConsumerSettings ReadConsumerSettingsFromConfig(string section)
        {
            var settings = new ConsumerSettings(brokers, ConsumerGroupId);
            configuration.GetSection(section).Bind(settings);

            return settings;
        }        
    }    
}