using System.Text;
using System.Text.Json;
using Interview.PaymentExecutor.Application;
using Microsoft.AspNetCore.Mvc.Formatters;
using Payment.Domain.Core;

namespace Interview.PaymentExecutor.Host.Services;

public sealed class BankService : IBankService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BankService(IHttpClientFactory httpClientFactory) 
        => _httpClientFactory = httpClientFactory;

    public async Task Pay(        
        PaymentId paymentId,
        CardInformation cardInformation,
        Money amount,
        MerchantId merchantId, 
        CancellationToken token)
    {
        var content = JsonSerializer.Serialize(GetPaymentRequest(paymentId, cardInformation, amount, merchantId));
        
        using var httpClient = _httpClientFactory.CreateClient(nameof(BankService));
        var request = new HttpRequestMessage(HttpMethod.Post, $"bank/pay")
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(request, token);

        if ((int)response.StatusCode > 299)
            throw new ApplicationException(await GetErrorDetails(response, token));
    }

    private static PaymentRequest GetPaymentRequest(
        PaymentId paymentId,
        CardInformation cardInformation,
        Money amount,
        MerchantId merchantId)
    {
        return new PaymentRequest
        {
            PaymentId = paymentId.Value,
          
            CardNumber = cardInformation.CardNumber.Value,
            Expiry = cardInformation.Expiry.Value,
            Cvv = cardInformation.Cvv.Value,
            CardHolder = cardInformation.CardHolder,
            
            Amount = amount.Amount,
            Currency = amount.Currency.ToString(),
            
            MerchantId = merchantId.Value
        };
    }
    
    private static async Task<string> GetErrorDetails(HttpResponseMessage response, CancellationToken token)
    {
        var responseContent = await response.Content.ReadAsStringAsync(token);
        var error = JsonSerializer.Deserialize<Error>(responseContent);

        return error?.ErrorReason ?? responseContent;
    }

    private sealed class Error
    {
        public string? ErrorReason { get; set; }
    }
    
    private sealed class PaymentRequest
    {
        public string PaymentId { get; set; } = null!;
    
        public string CardNumber { get; set; } = null!;
        public string Expiry { get; set; } = null!;
        public string Cvv { get; set; } = null!;
        public string? CardHolder { get; set; }
    
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
    
        public string MerchantId { get; set; } = null!;
    }    
}