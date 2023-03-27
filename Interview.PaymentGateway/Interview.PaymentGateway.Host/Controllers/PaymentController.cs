using Interview.PaymentGateway.Application;
using Interview.PaymentGateway.Host.Controllers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Host.Controllers;

[Route("payment_gateway")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService) 
        => _paymentService = paymentService;

    [HttpGet("get_id")]
    public async Task<GetPaymentIdResponse> GetId(string merchantId, CancellationToken token)
    {
        // merchantId Can be used for security
        var paymentId = await _paymentService.GenerateId(new MerchantId(merchantId), token);
        return new GetPaymentIdResponse { PaymentId = paymentId.Value };
    }
    
    [HttpPost("pay")]
    public async Task Pay([FromBody] PaymentRequest request, CancellationToken token)
    {
        await _paymentService.Pay(
            new PaymentId(request.PaymentId), 
            new CardInformation(
                new CardNumber(request.CardNumber), 
                new Expiry(request.Expiry), 
                new Cvv(request.Cvv), 
                request.CardHolder),
            new Money(request.Amount, CurrencyConverter.Convert(request.Currency)),
            new MerchantId(request.MerchantId),
            token);
    }
}