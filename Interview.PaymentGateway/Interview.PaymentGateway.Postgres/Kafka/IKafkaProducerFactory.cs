using Confluent.Kafka;

namespace Interview.PaymentGateway.Postgres.Kafka;

public interface IKafkaProducerFactory<TKey, TValue>
{
    IProducer<TKey, TValue> Create();
}
