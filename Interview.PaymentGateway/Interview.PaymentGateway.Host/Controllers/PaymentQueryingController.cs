using Interview.PaymentGateway.Application.Querying;
using Interview.PaymentGateway.Host.Controllers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Host.Controllers;

[Route("payments")]
public sealed class PaymentQueryingController : ControllerBase
{
    private readonly IPaymentQueryService _paymentQueryService;

    public PaymentQueryingController(IPaymentQueryService paymentQueryService) 
        => _paymentQueryService = paymentQueryService;

    [HttpGet]
    public async Task<GetPaymentInformationResponse> GetId(string merchantId, string paymentId, CancellationToken token)
    {
        var paymentInformation = await _paymentQueryService.Get(new PaymentId(paymentId), merchantId, token);
        return Convert(paymentInformation);
    }

    private static GetPaymentInformationResponse Convert(PaymentInformation paymentInformation)
    {
        return new GetPaymentInformationResponse
        {
            PaymentId = paymentInformation.PaymentId.Value,
            
            MaskedCardNumber = paymentInformation.CardNumber.Mask(),
            Expiry = paymentInformation.Expiry.Value,
            CardHolder = paymentInformation.CardHolder,
            
            Amount = paymentInformation.Amount.Amount,
            Currency = paymentInformation.Amount.Currency.ToString(),

            Date = paymentInformation.Date,
            
            Status = paymentInformation.Status.ToString(),
            Reason = paymentInformation.Reason
        };
    }
}