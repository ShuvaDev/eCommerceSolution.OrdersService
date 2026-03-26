namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public record ProductDeletionMessage(Guid ProductId, string? ProductName);
