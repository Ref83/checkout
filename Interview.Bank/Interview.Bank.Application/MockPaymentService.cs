using Payment.Domain.Core;

namespace Interview.Bank.Application;

public sealed class MockPaymentService : IPaymentService
{
    private const string Scammer = "scammer";
    private const string NotEnoughMoney = "empty account";
    private const string Timeout = "timeout";
    
    public async Task Pay(PaymentInformation paymentInformation, CancellationToken token)
    {
        await Task.CompletedTask; // Do some work

        if (paymentInformation.CardInformation.CardHolder?.Equals(Scammer) == true)
            throw  new ApplicationException("Fraud transaction detected."); 
        
        if (paymentInformation.CardInformation.CardHolder?.Equals(NotEnoughMoney) == true)
            throw new ApplicationException("Not enough money.");

        if (paymentInformation.CardInformation.CardHolder?.Equals(Timeout) == true)
            await Task.Delay(TimeSpan.FromMinutes(1), token);
    }
}

