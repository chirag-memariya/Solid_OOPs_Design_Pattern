namespace EfCoreCacheAsidedemo.Messaging.Events;
public record ProductCreatedEvent
(
    int Id,
    string Name,
    decimal Price
);