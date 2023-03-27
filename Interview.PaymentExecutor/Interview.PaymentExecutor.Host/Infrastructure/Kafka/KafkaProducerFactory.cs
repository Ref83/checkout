using Confluent.Kafka;

namespace Interview.PaymentExecutor.Host.Infrastructure.Kafka;

public sealed class KafkaProducerFactory<TKey, TValue> : IKafkaProducerFactory<TKey, TValue>
{
    private readonly string _brokers;

    public KafkaProducerFactory(string brokers)
        => _brokers = brokers;

    public IProducer<TKey, TValue> Create()
    {
        var producerConfiguration = new ProducerConfig
        {
            BootstrapServers = _brokers,
            Acks = Acks.All,
            RequestTimeoutMs = 3 * 60 * 1000,
            LingerMs = 300,
            EnableIdempotence = true,
            MaxInFlight = 1,
            TransactionalId = Guid.NewGuid().ToString()
        };

        var producer = new ProducerBuilder<TKey, TValue>(producerConfiguration).Build();
        producer.InitTransactions(TimeSpan.FromSeconds(30));

        return producer;
    }
}
