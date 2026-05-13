namespace OOPA;

// ABSTRACTION: Hiding internal implementation details and exposing only
// what the caller needs to know(necessary functionality.) — via interfaces or abstract classes.

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