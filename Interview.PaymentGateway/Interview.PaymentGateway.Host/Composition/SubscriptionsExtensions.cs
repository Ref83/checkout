using Eventso.Subscription.Hosting;
using Eventso.Subscription.Kafka;
using Eventso.Subscription.SpanJson;
using Interview.PaymentGateway.Application.Events;
using Interview.PaymentGateway.Application.Handlers;

namespace Interview.PaymentGateway.Host.Composition;

public static class SubscriptionsExtensions
{
    private const string ConsumerGroupId = "payment_gateway";
    private const string PaymentCompletedTopicConfigurationSection = "PaymentCompletedConsumer";
    private const string PaymentRejectedTopicConfigurationSection = "PaymentRejectedConsumer";

    public static IServiceCollection ConfigureSubscriptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var brokers = configuration.GetSection("Kafka:Brokers").Get<string>();

        return services
            .AddSubscriptions(
                (s, _) => s
                    .Add(
                        ReadConsumerSettingsFromConfig(PaymentCompletedTopicConfigurationSection),
                        new JsonMessageDeserializer<PaymentCompleted>())
                    .Add(
                        ReadConsumerSettingsFromConfig(PaymentRejectedTopicConfigurationSection),
                        new JsonMessageDeserializer<PaymentRejected>()),
                handlersSelector: selector =>
                    selector.FromAssembliesOf(typeof(PaymentHandler)));

        ConsumerSettings ReadConsumerSettingsFromConfig(string section)
        {
            var settings = new ConsumerSettings(brokers, ConsumerGroupId);
            configuration.GetSection(section).Bind(settings);

            return settings;
        }        
    }
}