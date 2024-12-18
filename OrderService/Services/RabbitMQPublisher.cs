using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderService.Services
{
    public class RabbitMQPublisher
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "orderQueue";

        public async Task PublishAsync<T>(T message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname };

            await using var connection = await factory.CreateConnectionAsync();

            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = new BasicProperties();

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _queueName,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }
    }
}