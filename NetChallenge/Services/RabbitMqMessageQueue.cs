using NetChallenge.Models;
using RabbitMQ.Client;
using System.Text;

namespace NetChallenge.Services
{
    public class RabbitMqMessageQueue : IMessageQueue
    {
        public void Publish(string stockCode, string chatId)
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "stock_requests",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = new StockRequest() { StockCode = stockCode, ChatId = chatId };
            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

            channel.BasicPublish(exchange: "",
                                 routingKey: "stock_requests",
                                 basicProperties: null,
                                 body: body);
        }
    }
}
