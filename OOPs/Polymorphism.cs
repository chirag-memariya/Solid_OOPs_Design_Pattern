
namespace OOP;

// POLYMORPHISM: The ability of one interface or method to behave differently
// depending on context — either at compile time (overloading) or runtime (overriding).

// ── 1. Compile-Time Polymorphism: Method Overloading ──────────────────────────
// Same method name, different parameter signatures — resolved at compile time.

public class EmailService
{
    public void Send(string message)
        => Console.WriteLine($"Email sent: {message}");

    public void Send(string message, string subject)
        => Console.WriteLine($"[{subject}] {message}");

    public void Send(string message, string subject, string recipient)
        => Console.WriteLine($"To: {recipient} | [{subject}] {message}");
}

// ── 2. Runtime Polymorphism: Method Overriding ────────────────────────────────
// Subclasses override a base method — the correct version is resolved at runtime
// based on the actual object type, not the declared type.

public class BaseEmailService
{
    public virtual void Send(string message)
        => Console.WriteLine($"[Base]    Email sent: {message}");
}

public class GmailService : BaseEmailService
{
    public override void Send(string message)
        => Console.WriteLine($"[Gmail]   Email sent: {message}");
}

public class OutlookService : BaseEmailService
{
    public override void Send(string message)
        => Console.WriteLine($"[Outlook] Email sent: {message}");
}

public class Program
{
    public static void Main()
    {
        // ── Overloading Demo (Compile-Time) ─────────────────────────────
        var email = new EmailService();

        email.Send("Hello");
        email.Send("Hello", "Greetings");
        email.Send("Hello", "Greetings", "Alice");

        Console.WriteLine();

        // ── Overriding Demo (Runtime) ────────────────────────────────────
        // Declared as BaseEmailService, but actual type determines which Send() runs
        BaseEmailService gmail   = new GmailService();
        BaseEmailService outlook = new OutlookService();

        gmail.Send("Hello via Gmail");     // → GmailService.Send()   at runtime
        outlook.Send("Hello via Outlook"); // → OutlookService.Send() at runtime

        Console.WriteLine();

        // ── Polymorphic Collection ────────────────────────────────────────
        // The real power: one loop, different behaviors per type
        BaseEmailService[] services = { new GmailService(), new OutlookService() };
        foreach (var service in services)
            service.Send("Broadcast message"); // each calls its own override
    }
}
// **Key fixes & improvements:**

// - **`EmailService1`** renamed to `EmailService` — the `1` suffix was a leftover artifact
// - **Expression-bodied methods** used throughout — removes noisy single-line braces
// - **Aligned output prefixes** (`[Base]`, `[Gmail]`, `[Outlook]`) — makes runtime dispatch visually obvious when you run it
// - **`Console.WriteLine()`** spacers added between sections for readable output
// - **Polymorphic collection demo added** — a `BaseEmailService[]` array showing the real-world power of runtime polymorphism: one loop, different behavior per type
// - **Comments rewritten** to clearly distinguish compile-time vs runtime polymorphism and *why* each exists