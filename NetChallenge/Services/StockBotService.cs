using Microsoft.AspNetCore.SignalR;
using NetChallenge.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NetChallenge.Services
{
    public class StockBotService : BackgroundService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public StockBotService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "stock_requests",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Deserialize JSON
                var request = JsonSerializer.Deserialize<StockRequest>(message);

                if (request is not null)
                {
                    var stockCode = request.StockCode;
                    var chatId = request.ChatId;

                    // Fetch and parse the stock quote
                    var stockQuote = await GetStockQuoteAsync(stockCode);

                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // Store the message in the corresponding chat
                    if (ChatHub.Chats.TryGetValue(chatId, out var messages))
                    {
                        messages.Add(new Message
                        {
                            Timestamp = timestamp,
                            User = "StockBot",
                            MessageText = stockQuote
                        });

                        if (messages.Count > 50) messages.RemoveAt(0);
                    }

                    // Send the response to the chatroom
                    await _hubContext.Clients.Group(chatId).SendAsync("ReceiveMessage", chatId, "StockBot", stockQuote, timestamp);
                }
            };

            channel.BasicConsume(queue: "stock_requests", autoAck: true, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken); // Delay to prevent high CPU usage
            }
        }

        private async Task<string> GetStockQuoteAsync(string stockCode)
        {
            using var client = new HttpClient();
            var url = $"https://stooq.com/q/l/?s={stockCode}&f=sd2t2ohlcv&h&e=csv";

            try
            {
                var response = await client.GetStringAsync(url);
                return ParseStockCsvResponse(response, stockCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StockBot] Error fetching stock data: {ex.Message}");
                return $"StockBot: Error occurred while fetching stock data for {stockCode}.";
            }
        }

        public string ParseStockCsvResponse(string response, string stockCode)
        {
            var lines = response.Split('\n');

            // Ensure there is data beyond the header
            if (lines.Length < 2)
            {
                return $"StockBot: Failed to fetch stock quote for {stockCode}.";
            }

            // Parse the data line
            var data = lines[1].Split(',');
            if (data.Length >= 8 && decimal.TryParse(data[6], out var closePrice))
            {
                return $"{data[0]} quote is ${closePrice} per share.";
            }

            return $"StockBot: Invalid stock data for {stockCode}.";
        }
    }
}
