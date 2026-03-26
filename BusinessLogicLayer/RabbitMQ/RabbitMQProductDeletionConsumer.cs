using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionConsumer : IDisposable, IRabbitMQProductDeletionConsumer
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly IDistributedCache _distributedCache;

    public RabbitMQProductDeletionConsumer(IConfiguration configuration, IDistributedCache distributedCache)
    {
        _configuration = configuration;
        _distributedCache = distributedCache;

        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string port = _configuration["RabbitMQ_Port"]!;

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };
        _connection = connectionFactory.CreateConnection();

        _channel = _connection.CreateModel();
    }


    public void Consume()
    {
        string routingKey = "product.delete";
        string queueName = "orders.product.delete.queue";

        // Create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        // Create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); //x-message-ttl | x-max-length | x-expired 

        // Bind the message to exchange
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            ProductDeletionMessage productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);

            if (productDeletionMessage != null)
            {
                await HandleProductDeletion(productDeletionMessage.ProductId);
            }
        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }
    private async Task HandleProductDeletion(Guid productID)
    {
        string cacheKeyToDelete = $"product:{productID}";
        await _distributedCache.RemoveAsync(cacheKeyToDelete);
    }
    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}