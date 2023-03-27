using Eventso.Subscription;
using Interview.PaymentExecutor.Application.Events;
using Microsoft.Extensions.Logging;
using Payment.Domain.Core;
using Polly;
using Polly.Retry;

namespace Interview.PaymentExecutor.Application.Handlers;

public sealed class PaymentsHandler : 
    IMessageHandler<PaymentCreated>,
    IMessageHandler<PaymentFailed>
{
    private readonly IBankService _bankService;
    private readonly IPaymentRetryService _paymentRetryService;
    private readonly IDeadEndMessageService _deadEndMessageService;
    private readonly ILogger<PaymentsHandler> _logger;
    private readonly IPaymentCompletedNotifier _paymentCompletedNotifier;
    private readonly RetryPolicy _retryPolicy;

    public PaymentsHandler(
        IBankService bankService, 
        IPaymentRetryService paymentRetryService, 
        IDeadEndMessageService deadEndMessageService,
        IPaymentCompletedNotifier paymentCompletedNotifier,
        ILogger<PaymentsHandler> logger)
    {
        _bankService = bankService;
        _paymentRetryService = paymentRetryService;
        _deadEndMessageService = deadEndMessageService;
        _paymentCompletedNotifier = paymentCompletedNotifier;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .Retry(
                retryCount: 3,
                onRetry: (ex, _) => logger.LogWarning($"Payment failed : {ex}."));
    }

    public async Task Handle(PaymentCreated message, CancellationToken token)
    {
        try
        {
            var paymentId = new PaymentId(message.PaymentId); 
            
            await _bankService.Pay(
                paymentId,
                new CardInformation(
                    new CardNumber(message.CardNumber), 
                    new Expiry(message.Expiry), 
                    new Cvv(message.Cvv), 
                    message.CardHolder),
                new Money(message.Amount, Convert(message.Currency)),
                new MerchantId(message.MerchantId), 
                token);
            
            await _paymentCompletedNotifier.Notify(paymentId, token);
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception, "Can't handle payment");
            await _deadEndMessageService.Send(message.PaymentId, exception.Message, token);
        }
        catch (Exception)
        {
            await _paymentRetryService.Send(message, token);
        }
    }
    
    public async Task Handle(PaymentFailed message, CancellationToken token)
    {
        try
        {
            var paymentId = new PaymentId(message.PaymentId); 
            
            await _retryPolicy.Execute(async () => await _bankService.Pay(
                new PaymentId(message.PaymentId),
                new CardInformation(
                    new CardNumber(message.CardNumber), 
                    new Expiry(message.Expiry), 
                    new Cvv(message.Cvv), 
                    message.CardHolder),
                new Money(message.Amount, Convert(message.Currency)),
                new MerchantId(message.MerchantId), 
                token));
            
            await _paymentCompletedNotifier.Notify(paymentId, token);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Can't handle payment");
            await _deadEndMessageService.Send(message.PaymentId,  exception.Message, token);
        }
    }

    private static Currency Convert(string code)
    {
        if (!Enum.TryParse<Currency>(code, true, out var currency))
            throw new ArgumentException("Unknown currency: " + code, nameof(code));

        return currency;
    }
}
