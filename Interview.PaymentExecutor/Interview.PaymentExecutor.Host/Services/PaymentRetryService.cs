using System.Text.Json;
using Interview.PaymentExecutor.Application;
using Interview.PaymentExecutor.Application.Events;
using Interview.PaymentExecutor.Host.Infrastructure.Kafka;
using Interview.PaymentExecutor.Host.Services.Messages;

namespace Interview.PaymentExecutor.Host.Services;

public sealed class PaymentRetryService : IPaymentRetryService
{
    private readonly IKafkaProducerFactory<string, string> _kafkaProducerFactory;
    private readonly string _topic;

    public PaymentRetryService(IKafkaProducerFactory<string, string> kafkaProducerFactory, string topic)
    {
        _kafkaProducerFactory = kafkaProducerFactory;
        _topic = topic;
    }

    public async Task Send(PaymentCreated message, CancellationToken token)
    {
        var producer = _kafkaProducerFactory.Create();
        var paymentFailedMessage = CreateMessage(message);
        await producer.SendMessage(
            _topic, 
            message.PaymentId, 
            JsonSerializer.Serialize(paymentFailedMessage), 
            token);
    }

    private static PaymentFailed CreateMessage(PaymentCreated message)
    {
        return new PaymentFailed
        {
            PaymentId = message.PaymentId,
            
            CardNumber = message.CardNumber,
            Expiry = message.Expiry,
            Cvv = message.Cvv,
            CardHolder = message.CardHolder,
            
            Amount = message.Amount,
            Currency = message.Currency,
            
            MerchantId = message.MerchantId                
        };
    }
}