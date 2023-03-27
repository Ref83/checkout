using System.Text.Json;
using Dapper;
using Interview.PaymentGateway.Domain;
using Interview.PaymentGateway.Postgres.Kafka;
using Npgsql;
using NpgsqlTypes;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Postgres;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly OutboxEventPublisher _outboxEventPublisher;

    public PaymentRepository(IConnectionFactory connectionFactory, OutboxEventPublisher outboxEventPublisher)
    {
        _connectionFactory = connectionFactory;
        _outboxEventPublisher = outboxEventPublisher;
    }

    public async Task<Domain.Payment> Get(PaymentId id, CancellationToken token)
    {
        const string sql = @"
            select
                id,
                card_number,
                expiry,
                cvv,
                card_holder,
                amount,
                currency,
                merchant_id,
                status,
                reason,
                xmin as version
            from payments
            where id = @id;"; 
        
        await using var connection = _connectionFactory.Create();

        var command = new CommandDefinition(sql, new {id = id.Value}, cancellationToken: token);

        var paymentData = await connection.QuerySingleOrDefaultAsync<PaymentData>(command)
                  ?? throw new ApplicationException($"Cannot find payment with id {id}");

        return Convert(paymentData);
    }

    public async Task<bool> TryAdd(Domain.Payment payment, Event @event, CancellationToken token)
    {
        const string sql = @"
            insert into outbox(key, data, topic)            
            select @id, @messageData, @topic
            where not exists(select 1 from payments where id = @id);

            insert into payments(id, card_number, expiry, cvv, card_holder, amount, currency, merchant_id, status, reason)
            values(@id, @cardNumber, @expiry, @cvv, @cardHolder, @amount, @currency, @merchantId, @status, @reason)
            on conflict (id) do nothing             
            returning xmin;";

        var added = false;
        
        await using var connection = _connectionFactory.Create();

        var messageData = JsonSerializer.Serialize(@event.Message);
        
        await using var command = new NpgsqlCommand
        {
            CommandText = sql,
            Parameters =
            {
                new NpgsqlParameter<string>("id", payment.Id.Value),
                new NpgsqlParameter<string>("cardNumber", payment.Card.CardNumber.Value),
                new NpgsqlParameter<string>("expiry", payment.Card.Expiry.Value),
                new NpgsqlParameter<string>("cvv", payment.Card.Cvv.Value),
                new NpgsqlParameter("cardHolder", NpgsqlDbType.Text) {Value = ToNullable(payment.Card.CardHolder)},
                new NpgsqlParameter<decimal>("amount", payment.Amount.Amount),
                new NpgsqlParameter<string>("currency", payment.Amount.Currency.ToString()),
                new NpgsqlParameter<string>("merchantId", payment.MerchantId.Value),
                new NpgsqlParameter<short>("status", (short)payment.Status),
                new NpgsqlParameter("reason", NpgsqlDbType.Text) {Value = ToNullable(payment.Reason)},
                new NpgsqlParameter<uint>("version", NpgsqlDbType.Xid) {Value = payment.Version},

                new NpgsqlParameter<string>("messageData", messageData),
                new NpgsqlParameter<string>("topic", @event.Topic),
            },
            Connection = connection
        };

        await connection.OpenAsync(token);

        await using var transaction = await connection.BeginTransactionAsync(token);

        await using var reader = await command.ExecuteReaderAsync(token);

        if (await reader.ReadAsync(token))
        {
            payment.Version = reader.GetFieldValue<uint>(0);
            added = true;
        }

        await reader.DisposeAsync();
        await transaction.CommitAsync(token);

        await _outboxEventPublisher.Publish(token);

        return added;
    }

    public async Task Update(Domain.Payment payment, CancellationToken token = default)
    {
        const string sql = @"
            update payments set 
                status = @status, 
                reason = @reason
            where id = @id and xmin = @version
            returning xmin;";
        
        await using var connection = _connectionFactory.Create();

        await using var command = new NpgsqlCommand
        {
            CommandText = sql,
            Parameters =
            {
                new NpgsqlParameter<string>("id", payment.Id.Value),
                new NpgsqlParameter<short>("status", (short)payment.Status),
                new NpgsqlParameter("reason", NpgsqlDbType.Text) {Value = ToNullable(payment.Reason)},
                new NpgsqlParameter<uint>("version", NpgsqlDbType.Xid) {Value = payment.Version},
            },
            Connection = connection
        };

        await connection.OpenAsync(token);

        await using var transaction = await connection.BeginTransactionAsync(token);

        await using var reader = await command.ExecuteReaderAsync(token);

        if (!await reader.ReadAsync(token))
            throw new ConcurrencyException($"Concurrency conflict occured when saving payment with id = {payment.Id}");

        payment.Version = reader.GetFieldValue<uint>(0);

        await reader.DisposeAsync();
        await transaction.CommitAsync(token);        
    }
    
    private static Domain.Payment Convert(PaymentData paymentData)
    {
        return new Domain.Payment(
            new PaymentId(paymentData.id),
            new CardInformation(
                new CardNumber(paymentData.card_number),
                new Expiry(paymentData.expiry),
                new Cvv(paymentData.cvv),
                paymentData.card_holder),
            new Money(paymentData.amount, CurrencyConverter.Convert(paymentData.currency)),
            new MerchantId(paymentData.merchant_id),
            (PaymentStatus)paymentData.status,
            paymentData.reason,
            paymentData.version);
    }
    
    private object? ToNullable<T>(T? value)
        => (object?) value ?? DBNull.Value;
    
    private sealed class PaymentData
    {
        public string id { get; set; } = null!;
    
        public string card_number { get; set; } = null!;
        public string expiry { get; set; } = null!;
        public string cvv { get; set; } = null!;
        public string? card_holder { get; set; }
    
        public decimal amount { get; set; }
        public string currency { get; set; } = null!;
    
        public string merchant_id { get; set; } = null!;
        
        public short status { get; set; }
        public string? reason { get; set; }
        
        public uint version { get; set; }
    }
}