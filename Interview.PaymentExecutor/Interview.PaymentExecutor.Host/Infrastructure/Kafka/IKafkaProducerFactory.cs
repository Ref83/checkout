using Confluent.Kafka;

namespace Interview.PaymentExecutor.Host.Infrastructure.Kafka;

public interface IKafkaProducerFactory<TKey, TValue>
{
    IProducer<TKey, TValue> Create();
}
