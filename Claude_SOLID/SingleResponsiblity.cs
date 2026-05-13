namespace OOP;

// SINGLE RESPONSIBILITY PRINCIPLE (SRP):
// A class should have only ONE reason to change —
// meaning it should do one job and do it well.
// If a class handles multiple concerns, split it into focused classes.

// ── ❌ BAD: One class doing everything ───────────────────────────────────────

public class BadOrderService
{
    public void PlaceOrder(string item)
    {
        // 1. Business logic
        Console.WriteLine($"Order placed for: {item}");

        // 2. Database concern
        Console.WriteLine($"Saving order to database...");

        // 3. Email concern
        Console.WriteLine($"Sending confirmation email for: {item}");

        // 4. Logging concern
        Console.WriteLine($"[LOG] Order processed for: {item}");
    }
}
// ☠ If email logic changes   → you touch OrderService
// ☠ If DB logic changes      → you touch OrderService
// ☠ If logging changes       → you touch OrderService
// One class, four reasons to change. Violates SRP.

// ── ✅ GOOD: Each class has one responsibility ────────────────────────────────

// Handles only order business logic
public class OrderService
{
    private readonly OrderRepository _repo;
    private readonly EmailService    _email;
    private readonly LogService      _logger;

    public OrderService(OrderRepository repo, EmailService email, LogService logger)
    {
        _repo   = repo;
        _email  = email;
        _logger = logger;
    }

    public void PlaceOrder(string item)
    {
        _repo.Save(item);
        _email.SendConfirmation(item);
        _logger.Log($"Order placed for: {item}");
    }
}

// Handles only database operations
public class OrderRepository
{
    public void Save(string item)
        => Console.WriteLine($"[DB]    Saving order: {item}");
}

// Handles only email delivery
public class EmailService
{
    public void SendConfirmation(string item)
        => Console.WriteLine($"[Email] Confirmation sent for: {item}");
}

// Handles only logging
public class LogService
{
    public void Log(string message)
        => Console.WriteLine($"[Log]   {message}");
}

public class Program
{
    public static void Main()
    {
        // ── SRP Violation Demo ───────────────────────────────────────────
        Console.WriteLine("=== ❌ BAD (No SRP) ===");
        var bad = new BadOrderService();
        bad.PlaceOrder("Laptop");

        Console.WriteLine();

        // ── SRP Compliant Demo ───────────────────────────────────────────
        Console.WriteLine("=== ✅ GOOD (SRP Applied) ===");
        var repo   = new OrderRepository();
        var email  = new EmailService();
        var logger = new LogService();

        var order = new OrderService(repo, email, logger);
        order.PlaceOrder("Laptop");

        // Now if email logic changes  → only touch EmailService
        // If DB logic changes         → only touch OrderRepository
        // If logging changes          → only touch LogService
        // OrderService never needs to change for infrastructure reasons
    }
}

// **What this teaches:**

// - **Bad example first** — shows exactly what SRP violation looks like and *why* it hurts (4 reasons to change one class)
// - **Dependency Injection** — `OrderService` receives its collaborators via constructor, a natural consequence of splitting responsibilities
// - **Prefixed output** (`[DB]`, `[Email]`, `[Log]`) — makes it crystal clear which class is doing what at runtime
// - **Comments at the end** reinforce the payoff — each concern is now independently changeable without touching the others