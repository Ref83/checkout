using System.Text.Json;
using Interview.PaymentExecutor.Application;
using Interview.PaymentExecutor.Host.Infrastructure.Kafka;
using Interview.PaymentExecutor.Host.Services.Messages;
using Payment.Domain.Core;

namespace Interview.PaymentExecutor.Host.Services;

public sealed class PaymentCompletedNotifier : IPaymentCompletedNotifier
{
    private readonly IKafkaProducerFactory<string, string> _kafkaProducerFactory;
    private readonly string _topic;

    public PaymentCompletedNotifier(IKafkaProducerFactory<string, string> kafkaProducerFactory, string topic)
    {
        _kafkaProducerFactory = kafkaProducerFactory;
        _topic = topic;
    }

    public async Task Notify(PaymentId paymentId, CancellationToken token)
    {
        var producer = _kafkaProducerFactory.Create();
        var message = new PaymentCompleted {PaymentId = paymentId.Value};
        await producer.SendMessage(
            _topic, 
            paymentId.Value, 
            JsonSerializer.Serialize(message), 
            token);
    }
}