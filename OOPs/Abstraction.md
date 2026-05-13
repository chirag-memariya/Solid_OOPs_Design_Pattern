```csharp
namespace OOP;

// ABSTRACTION: Hiding internal implementation details and exposing only
// what the caller needs to know — via interfaces or abstract classes.

public interface IEmailService
{
    void Send(string message);
    void Schedule(string message, DateTime time);
}

public class EmailService : IEmailService
{
    // ✅ Exposed — part of the public contract (interface)
    public void Send(string message)
    {
        ConnectToSmtp();
        string formatted = FormatMessage(message);
        Console.WriteLine($"Email sent: {formatted}");
    }

    public void Schedule(string message, DateTime time)
    {
        string formatted = FormatMessage(message);
        Console.WriteLine($"Email scheduled at {time:g}: {formatted}");
    }

    // ❌ Hidden — internal implementation, invisible to the caller
    private string FormatMessage(string message) => $"[Formatted] {message}";
    private void ConnectToSmtp() => Console.WriteLine("Connecting to SMTP server...");
}

public class Program
{
    public static void Main()
    {
        // ── Abstraction Demo ────────────────────────────

        // Declared as IEmailService, not EmailService
        // → caller only sees what the interface exposes
        IEmailService email = new EmailService();

        email.Send("Hello World");
        email.Schedule("Meeting Reminder", DateTime.Now.AddHours(1));

        // email.FormatMessage("Test");  // ❌ Compile error — private method
        // email.ConnectToSmtp();        // ❌ Compile error — private method

        // Even if declared as EmailService directly,
        // private methods are still inaccessible:
        EmailService direct = new EmailService();
        // direct.FormatMessage("Test"); // ❌ Still a compile error
    }
}
```

**Key fixes & improvements:**

- **Duplicate `namespace` declaration** removed — one namespace covers the whole file
- **Stray closing braces** fixed — `Main` and `Program` were malformed
- **`Send` reordered** — `ConnectToSmtp()` logically runs *before* formatting and sending
- **`:g` format specifier** added on `DateTime` — prints a clean short date/time (e.g. `3/17/2026 2:30 PM`)
- **Expression-bodied private methods** — single-line methods use `=>` for brevity
- **Extra demo added** — shows that even `EmailService direct` can't access private methods, not just the interface reference
- **Comments** rewritten to clearly explain the *why* of abstraction, not just label the code