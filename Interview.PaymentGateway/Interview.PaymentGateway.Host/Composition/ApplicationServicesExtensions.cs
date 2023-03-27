
using Interview.PaymentGateway.Application;
using Interview.PaymentGateway.Domain;

namespace Interview.PaymentGateway.Host.Composition;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection ConfigureApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IPaymentService>(p => new PaymentService(
            p.GetRequiredService<IPaymentRepository>(),
            p.GetRequiredService<IPaymentHandledAwaiter>(),
            configuration.GetSection("PaymentCreatedTopic").Get<string>()
            ));
        services.AddSingleton<PaymentHandledNotifier>();
        services.AddSingleton<IPaymentHandledNotifier>(p => p.GetRequiredService<PaymentHandledNotifier>());
        services.AddSingleton<IPaymentHandledAwaiter>(p => p.GetRequiredService<PaymentHandledNotifier>());
        
        return services;
    }
}