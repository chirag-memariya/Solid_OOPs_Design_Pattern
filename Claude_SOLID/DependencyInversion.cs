namespace Code;

// DEPENDENCY INVERSION PRINCIPLE (DIP):
// 1. High-level modules should NOT depend on low-level modules.
//    Both should depend on abstractions (interfaces).
// 2. Abstractions should NOT depend on details.
//    Details (concrete classes) should depend on abstractions.
// In short: depend on interfaces, not concrete implementations.

// ── ❌ BAD: High-level class directly depends on low-level concrete class ──────

public class BadSmtpEmailSender
{
    public void Send(string message)
        => Console.WriteLine($"[SMTP]   Sending: {message}");
}

public class BadNotificationService
{
    private readonly BadSmtpEmailSender _sender = new BadSmtpEmailSender();
    // ☠ Hardwired to SMTP — cannot swap without modifying this class
    // ☠ Cannot test in isolation — always drags in BadSmtpEmailSender
    // ☠ High-level policy (notify) coupled to low-level detail (SMTP)
    // ☠ Violates DIP — concrete depends on concrete

    public void Notify(string message)
        => _sender.Send(message);
}

// ── ✅ GOOD: Both layers depend on an abstraction (interface) ─────────────────

// The abstraction — neither layer owns this, both depend on it
public interface IMessageSender
{
    void Send(string message);
}

// Low-level modules — details that depend on the abstraction
public class SmtpEmailSender : IMessageSender
{
    public void Send(string message)
        => Console.WriteLine($"[SMTP]    Sending: {message}");
}

public class SmsSender : IMessageSender
{
    public void Send(string message)
        => Console.WriteLine($"[SMS]     Sending: {message}");
}

public class PushNotificationSender : IMessageSender
{
    public void Send(string message)
        => Console.WriteLine($"[Push]    Sending: {message}");
}

// ✅ New channel tomorrow? Just ADD a new class — nothing existing touched
public class SlackSender : IMessageSender
{
    public void Send(string message)
        => Console.WriteLine($"[Slack]   Sending: {message}");
}

// High-level module — depends only on IMessageSender, not any concrete class
public class NotificationService
{
    private readonly IMessageSender _sender;

    // Dependency is INJECTED from outside — not created inside
    public NotificationService(IMessageSender sender)
        => _sender = sender;

    public void Notify(string message)
    {
        Console.WriteLine($"[Service] Notifying via {_sender.GetType().Name}...");
        _sender.Send(message);
    }
}

public class Program
{
    public static void Main()
    {
        // ── DIP Violation Demo ───────────────────────────────────────────
        Console.WriteLine("=== ❌ BAD (No DIP) ===");
        var bad = new BadNotificationService();
        bad.Notify("Server is down!");
        // Stuck with SMTP forever — no way to swap without editing the class

        Console.WriteLine();

        // ── DIP Compliant Demo ───────────────────────────────────────────
        Console.WriteLine("=== ✅ GOOD (DIP Applied) ===");

        // Swap the sender freely — NotificationService never changes
        IMessageSender[] senders =
        {
            new SmtpEmailSender(),
            new SmsSender(),
            new PushNotificationSender(),
            new SlackSender()           // ✅ New channel, zero changes to existing code
        };

        foreach (var sender in senders)
        {
            var service = new NotificationService(sender);
            service.Notify("Server is down!");
            Console.WriteLine();
        }

        // NotificationService never changed once.
        // Each new channel = one new class, nothing else.
    }
}

// **What this teaches:**

// - **Bad example** shows the classic DIP trap — `new BadSmtpEmailSender()` hardwired inside the high-level class, making it impossible to swap or test in isolation
// - **`IMessageSender`** is the abstraction both layers depend on — neither high-level nor low-level owns it
// - **Constructor injection** is the natural DIP mechanism — the dependency is handed in from outside, not created inside
// - **`SlackSender`** is the "new requirement" demo — added with zero changes to `NotificationService` or any existing sender
// - **`GetType().Name`** in `Notify()` makes the runtime type visible in output, clearly showing which concrete class is active without hardcoding names
// - **Ties back to OCP** — DIP + constructor injection naturally enables the Open/Closed Principle too