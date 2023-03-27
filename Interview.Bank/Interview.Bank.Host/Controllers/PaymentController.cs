using Interview.Bank.Application;
using Interview.Bank.Host.Controllers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Payment.Domain.Core;

namespace Interview.Bank.Host.Controllers;

[Route("bank")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService) 
        => _paymentService = paymentService;

    [HttpPost("pay")]
    public async Task Pay([FromBody] PaymentRequest request, CancellationToken token)
    {
        var paymentInformation = GetPaymentInformation(request); 
        await _paymentService.Pay(paymentInformation, token);
    }

    private static PaymentInformation GetPaymentInformation(PaymentRequest request)
    {
        return new PaymentInformation(
            new PaymentId(request.PaymentId),
            new CardInformation(
                new CardNumber(request.CardNumber), 
                new Expiry(request.Expiry), 
                new Cvv(request.Cvv), 
                request.CardHolder),
            new Money(request.Amount, Convert(request.Currency)),
            new MerchantId(request.MerchantId));
    }

    private static Currency Convert(string code)
    {
        if (!Enum.TryParse<Currency>(code, true, out var currency))
            throw new ArgumentException("Unknown currency: " + code, nameof(code));

        return currency;
    }
}