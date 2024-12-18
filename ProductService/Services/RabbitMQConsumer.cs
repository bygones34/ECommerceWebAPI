using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ProductService.Models;
using OrderService.Models;

namespace ProductService.Services
{
    public class RabbitMQConsumer
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "orderQueue";

        public async Task StartListeningAsync()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };

            await using var connection = await factory.CreateConnectionAsync();

            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var order = JsonSerializer.Deserialize<Order>(message);

                Console.WriteLine($"Received Order: {order.Id}, Customer: {order.CustomerName}, Total: {order.TotalAmount}");

                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Listening for messages...");
        }
    }
}