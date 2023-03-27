using System.Text.Json;
using Interview.PaymentExecutor.Application;
using Interview.PaymentExecutor.Host.Infrastructure.Kafka;
using Interview.PaymentExecutor.Host.Services.Messages;

namespace Interview.PaymentExecutor.Host.Services;

public sealed class DeadEndMessageService : IDeadEndMessageService
{
    private readonly IKafkaProducerFactory<string, string> _kafkaProducerFactory;
    private readonly string _topic;

    public DeadEndMessageService(
        IKafkaProducerFactory<string, string> kafkaProducerFactory, 
        string topic)
    {
        _kafkaProducerFactory = kafkaProducerFactory;
        _topic = topic;
    }

    public async Task Send(string paymentId, string reason, CancellationToken token)
    {
        var producer = _kafkaProducerFactory.Create();
        var message = new PaymentRejected {PaymentId = paymentId, Reason = reason};
        await producer.SendMessage(
            _topic, 
            paymentId, 
            JsonSerializer.Serialize(message), 
            token);
    }
}