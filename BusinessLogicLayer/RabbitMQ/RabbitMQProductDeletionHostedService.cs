using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionHostedService : IHostedService
{
    private readonly IRabbitMQProductDeletionConsumer _rabbitMQProductDeletionConsumer;

    public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer rabbitMQProductDeletionConsumer)
    {
        _rabbitMQProductDeletionConsumer = rabbitMQProductDeletionConsumer;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rabbitMQProductDeletionConsumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQProductDeletionConsumer.Dispose();
        return Task.CompletedTask;
    }
}
