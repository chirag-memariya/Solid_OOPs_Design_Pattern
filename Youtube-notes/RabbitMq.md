Below is the **same Product flow**, but now **RabbitMQ is added as a message broker** to publish events (e.g., `ProductCreated`, `ProductUpdated`) while **cache-aside + EF Core + PostgreSQL** still remain the core data path.

This is the **most common real-world pattern**.

---

# Final Architecture (Clean & Practical)

**Request path (sync):**

```
Client → API → Redis (cache) → PostgreSQL (EF Core)
```

**Event path (async):**

```
API → RabbitMQ → Consumers (email, search index, audit, cache invalidation, etc.)
```
RabbitMQ is **NOT** used for CRUD directly.
It is used for **event-driven communication**.

---

## 1. Run PostgreSQL + Redis + RabbitMQ (Docker)

```bash
docker run -d --name postgres \
  -e POSTGRES_USER=appuser \
  -e POSTGRES_PASSWORD=apppassword \
  -e POSTGRES_DB=cache_demo_db \
  -p 5432:5432 postgres:16

docker run -d --name redis -p 6379:6379 redis

docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

RabbitMQ UI:

```
http://localhost:15672
user: guest
pass: guest
```

---

## 2. Add NuGet Packages

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package RabbitMQ.Client
```

---

## 3. RabbitMQ Settings

`appsettings.json`

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "Exchange": "product.events"
  }
}
```

---

## 4. Create Event Contracts (Very Important)

`Messaging/Events/ProductCreatedEvent.cs`

```csharp
namespace EfCoreCacheAsidePostgres.Messaging.Events;

public record ProductCreatedEvent(
    int Id,
    string Name,
    decimal Price);
```

---

## 5. Create RabbitMQ Publisher (Infrastructure Layer)

`Messaging/RabbitMqPublisher.cs`

```csharp
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace EfCoreCacheAsidePostgres.Messaging;

public interface IEventPublisher
{
    void Publish<T>(T @event);
}

public class RabbitMqPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly string _exchange;

    public RabbitMqPublisher(IConfiguration config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"],
            UserName = config["RabbitMQ:Username"],
            Password = config["RabbitMQ:Password"]
        };

        _exchange = config["RabbitMQ:Exchange"];
        _connection = factory.CreateConnection();
    }

    public void Publish<T>(T @event)
    {
        using var channel = _connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: _exchange,
            type: ExchangeType.Fanout,
            durable: true);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(@event));

        channel.BasicPublish(
            exchange: _exchange,
            routingKey: string.Empty,
            body: body);
    }
}
```

---

## 6. Register RabbitMQ Publisher (`Program.cs`)

```csharp
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();
```

---

## 7. Update Product POST API (Publish Event)

`Controllers/ProductController.cs`

```csharp
[HttpPost]
public async Task<IActionResult> Create(Product product)
{
    _db.Products.Add(product);
    await _db.SaveChangesAsync();

    // Cache-aside rule: optional cache
    await _cache.SetAsync(
        $"product_{product.Id}",
        product,
        TimeSpan.FromMinutes(5));

    // Publish event
    _publisher.Publish(new ProductCreatedEvent(
        product.Id,
        product.Name,
        product.Price));

    return CreatedAtAction(nameof(Get),
        new { id = product.Id },
        product);
}
```

Constructor update:

```csharp
private readonly IEventPublisher _publisher;

public ProductController(
    AppDbContext db,
    IDistributedCache cache,
    IEventPublisher publisher)
{
    _db = db;
    _cache = cache;
    _publisher = publisher;
}
```

---

## 8. What Happens Now (Flow)

### POST `/api/products`

1. Product saved to **PostgreSQL**
2. Cache updated / deferred
3. `ProductCreatedEvent` published to **RabbitMQ**
4. API returns immediately (no waiting)

---

## 9. Create a Consumer (Example)

This would usually be a **separate microservice**.

```csharp
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare("product.events", ExchangeType.Fanout, true);

var queue = channel.QueueDeclare().QueueName;
channel.QueueBind(queue, "product.events", "");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, args) =>
{
    var json = Encoding.UTF8.GetString(args.Body.ToArray());
    Console.WriteLine($"Event received: {json}");
};

channel.BasicConsume(queue, true, consumer);
```

Use cases:

* Email service
* Search indexing
* Audit logs
* Cache invalidation across services

---

## 10. Why RabbitMQ Is NOT Used for Reads

❌ Bad:

```
API → RabbitMQ → DB → API
```

✅ Correct:

```
API → DB (sync)
API → RabbitMQ (async event)
```

RabbitMQ is **event-driven**, not **query-driven**.

---

## 11. Cache + RabbitMQ Together (Best Practice)

| Action | DB     | Cache       | RabbitMQ      |
| ------ | ------ | ----------- | ------------- |
| POST   | Write  | Optional    | Publish event |
| GET    | Read   | Cache-aside | ❌             |
| PUT    | Update | Invalidate  | Publish event |
| DELETE | Delete | Invalidate  | Publish event |

---

## 12. Production Rules (Important)

* Events are **facts**, not commands
* Never rollback DB because RabbitMQ failed
* Use **Outbox Pattern** for guaranteed delivery
* One event = one responsibility

---

## 13. When to Use RabbitMQ Here

Use RabbitMQ when:

* Multiple consumers
* Async side effects
* Loose coupling
* High throughput

Do NOT use it for:

* CRUD responses
* Querying data
* Strong consistency workflows

---

### Next logical topics

* Outbox pattern with EF Core + RabbitMQ
* One-to-one vs fanout exchanges
* Cache invalidation via events
* Exactly-once vs at-least-once delivery

Tell me what you want next.
