using Interview.PaymentExecutor.Application;
using Interview.PaymentExecutor.Host.Infrastructure.Kafka;
using Interview.PaymentExecutor.Host.Services;

namespace Interview.PaymentExecutor.Host.Composition;

public static class ApplicationServicesExtension
{
    private const string PaymentRejectedTopicConfigurationSection = "PaymentRejectedTopic";
    private const string PaymentFailedTopicConfigurationSection = "PaymentFailedConsumer:Topic";
    private const string PaymentCompletedTopicConfigurationSection = "PaymentCompletedTopic";
    
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDeadEndMessageService>(
            p => new DeadEndMessageService(
                p.GetRequiredService<IKafkaProducerFactory<string, string>>(), 
                configuration.GetSection(PaymentRejectedTopicConfigurationSection).Get<string>()));
        
        services.AddSingleton<IPaymentCompletedNotifier>(
            p => new PaymentCompletedNotifier(
                p.GetRequiredService<IKafkaProducerFactory<string, string>>(), 
                configuration.GetSection(PaymentCompletedTopicConfigurationSection).Get<string>()));

        services.AddSingleton<IPaymentRetryService>(
            p => new PaymentRetryService(
                p.GetRequiredService<IKafkaProducerFactory<string, string>>(), 
                configuration.GetSection(PaymentFailedTopicConfigurationSection).Get<string>()));
        
        return services;
    }
}