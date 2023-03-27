
using Interview.Bank.Application;

namespace Interview.Bank.Host.Composition;

public static class ApplicationServicesExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentService, MockPaymentService>();
        
        return services;
    }
}