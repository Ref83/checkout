using Confluent.Kafka;

namespace Interview.PaymentExecutor.Host.Infrastructure.Kafka;

public static class ProducerExtensions
{
    public static async Task SendMessage(
        this IProducer<string, string> kafkaProducer,
        string topic,
        string key,
        string message,
        CancellationToken token)
    {
        kafkaProducer.BeginTransaction();

        try
        {
            var kafkaMessage = new Message<string, string>() { Key = key, Value = message };
            await kafkaProducer.ProduceAsync(topic, kafkaMessage, token);

            kafkaProducer.CommitTransaction();
        }
        catch (KafkaException)
        {
            kafkaProducer.AbortTransaction();
            throw;
        }
    }    
}