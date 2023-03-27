using Confluent.Kafka;
using Npgsql;

namespace Interview.PaymentGateway.Postgres.Kafka;

public sealed class OutboxEventPublisher
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IKafkaProducerFactory<string, string> _kafkaProducerFactory;

    public OutboxEventPublisher(
        IKafkaProducerFactory<string, string> kafkaProducerFactory,
        IConnectionFactory connectionFactory)
    {
        _kafkaProducerFactory = kafkaProducerFactory;
        _connectionFactory = connectionFactory;
    }

    public async Task Publish(CancellationToken token = default)
    {
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(token);

        while (true)
        {
            await using var transaction = await connection.BeginTransactionAsync(token);

            var outboxRecords = await GetOutboxRecords(connection, transaction, token);
            if (outboxRecords.Count == 0)
                return;

            await SendMessages(outboxRecords, kafkaProducer: _kafkaProducerFactory.Create(), token);

            await transaction.CommitAsync(token);
        }
    }

    private static async Task<IReadOnlyCollection<OutboxRecord>> GetOutboxRecords(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText
            = "DELETE FROM outbox WHERE id IN (SELECT id FROM outbox ORDER BY id LIMIT 100) RETURNING topic, key, data;";

        await using var reader = await command.ExecuteReaderAsync(token);

        var outboxRecords = new List<OutboxRecord>();
        while (await reader.ReadAsync(token))
        {
            var outboxRecord = new OutboxRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2));

            outboxRecords.Add(outboxRecord);
        }

        return outboxRecords;
    }

    private static async Task SendMessages(
        IReadOnlyCollection<OutboxRecord> outboxRecords,
        IProducer<string, string> kafkaProducer,
        CancellationToken token)
    {
        kafkaProducer.BeginTransaction();

        try
        {
            var produceTasks = outboxRecords
                .Select(i => kafkaProducer.ProduceAsync(i.Topic, CreateMessage(i.Key, i.Data), token));

            await Task.WhenAll(produceTasks);

            kafkaProducer.CommitTransaction();
        }
        catch (KafkaException)
        {
            kafkaProducer.AbortTransaction();
            throw;
        }

        static Message<string, string> CreateMessage(string key, string data)
            => new() {Key = key, Value = data};
    }

    private readonly record struct OutboxRecord(string Topic, string Key, string Data);
}