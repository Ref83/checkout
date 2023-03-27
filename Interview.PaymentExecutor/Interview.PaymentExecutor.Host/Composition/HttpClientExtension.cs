using Interview.PaymentExecutor.Application;
using Interview.PaymentExecutor.Host.Services;

namespace Interview.PaymentExecutor.Host.Composition;

public static class HttpClientExtension
{
    private const string BankServiceBaseAddressConfigurationSection = "BankService:BaseAddress";
    
    public static IServiceCollection ConfigureHttpServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IBankService, BankService>();

        var baseAddress = configuration.GetSection(BankServiceBaseAddressConfigurationSection).Get<string>();
        services
            .AddHttpClient(nameof(BankService), client =>
            {
                client.BaseAddress = new Uri(baseAddress);
            });

        return services;
    }    
}