using Dapper;
using Interview.PaymentGateway.Application.Querying;
using Interview.PaymentGateway.Domain;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Postgres;

public sealed class PaymentQueryService : IPaymentQueryService
{
    private readonly IConnectionFactory _connectionFactory;

    public PaymentQueryService(IConnectionFactory connectionFactory) 
        => _connectionFactory = connectionFactory;

    public async Task<PaymentInformation> Get(PaymentId paymentId, string merchantId, CancellationToken token)
    {
        const string sql = @"
            select
                id,
                card_number,
                expiry,
                card_holder,
                amount,
                currency,
                status,
                reason,
                created_at   
            from payments
            where id = @id 
            and merchant_id = @merchantId;"; 
        
        await using var connection = _connectionFactory.Create();

        var command = new CommandDefinition(sql, new {id = paymentId.Value, merchantId = merchantId});

        var paymentData = await connection.QuerySingleOrDefaultAsync<PaymentData>(command)
            ?? throw new ApplicationException($"Cannot find payment with id {paymentId} for merchantId = {merchantId}");

        return Convert(paymentData);
    }
    
    private static PaymentInformation Convert(PaymentData paymentData)
    {
        return new PaymentInformation(
            new PaymentId(paymentData.id),
            new CardNumber(paymentData.card_number),
            new Expiry(paymentData.expiry),
            paymentData.card_holder,
            new Money(paymentData.amount, CurrencyConverter.Convert(paymentData.currency)),
            paymentData.created_at,
            (PaymentStatus)paymentData.status,
            paymentData.reason);
    }    
    private sealed class PaymentData
    {
        public string id { get; set; } = null!;
    
        public string card_number { get; set; } = null!;
        public string expiry { get; set; } = null!;
        public string? card_holder { get; set; }
    
        public decimal amount { get; set; }
        public string currency { get; set; } = null!;
        
        public short status { get; set; }
        public string? reason { get; set; }
        
        public DateTime created_at { get; set; }
    }    
}