Here is the **cleanest, real-world EF Core + MediatR example** showing the **full flow with database save**.

This is exactly how real production systems use MediatR.

---

# 🔥 **FULL Real-World Example**

### Controller → Command → Pipeline (Validation) → Handler (EF Core) → DB Save → Response

---

# 1️⃣ **Command (DTO coming from API)**

```csharp
public record CreateUserCommand(string Name, string Email)
    : IRequest<Guid>;
```

This is the object you POST from UI.

---

# 2️⃣ **EF Core DbContext**

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<User> Users => Set<User>();
}

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
```

---

# 3️⃣ **Validation Pipeline (Global for ALL commands)**

```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is CreateUserCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.Name))
                throw new Exception("Name is required");

            if (string.IsNullOrWhiteSpace(cmd.Email))
                throw new Exception("Email is required");
        }

        return await next();
    }
}
```

📌 This runs BEFORE the handler.
📌 You write it once → applies to all your commands.

---

# 4️⃣ **Handler (Actual EF Core DB Work)**

```csharp
public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly AppDbContext _db;

    public CreateUserCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("🎯 Handler: creating user in EF Core");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        // Add to DB
        _db.Users.Add(user);

        // Save changes
        await _db.SaveChangesAsync(cancellationToken);

        Console.WriteLine("💾 User saved to DB");

        return user.Id;
    }
}
```

### Handler responsibilities:

✔ create entity
✔ populate fields
✔ save to database
✔ return new ID
❌ no validation
❌ no logging
❌ no controller logic

Pure business logic only.

---

# 5️⃣ **Controller (very clean)**

```csharp
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { UserId = id });
    }
}
```

📌 Controller only sends the command
📌 No service injection
📌 No EF Core
📌 No business logic

---

# 6️⃣ **Program.cs Registration**

```csharp
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly);
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

---

# 7️⃣ **Request Flow (Real World)**

```
POST /api/users
{
    "name": "Chirag",
    "email": "chirag@test.com"
}

↓
Controller
↓
mediator.Send(CreateUserCommand)
↓
ValidationBehavior (checks input)
↓
CreateUserCommandHandler
↓
EF Core: Add + SaveChanges
↓
returns Guid
↓
Controller returns 200 OK
```

---

# 🧠 **One-line summary**

> MediatR makes the controller clean, the EF Core logic isolated in the handler, and the validation reusable via pipelines.

---

If you want, I can also show:

✅ Real-world **UpdateUserCommand**
✅ Real-world **Query handler** (GetUserById using EF Core)
✅ Add **Transaction Behavior**
Just say: **"give update example"** or **"add transaction behavior"**.
