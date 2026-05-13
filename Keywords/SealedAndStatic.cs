namespace Code;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// SEALED vs STATIC — Two very different keywords, often confused
//
// ┌──────────┬────────────────────────────────────────────────────────┐
// │  sealed  │ Prevents INHERITANCE — class cannot be subclassed      │
// │          │ Can still be instantiated with `new`                   │
// │          │ Has instance fields, methods, constructors             │
// ├──────────┼────────────────────────────────────────────────────────┤
// │  static  │ Prevents INSTANTIATION — cannot use `new`             │
// │          │ Everything inside must also be static                  │
// │          │ Lives for the entire lifetime of the application       │
// └──────────┴────────────────────────────────────────────────────────┘
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// PART 1 — SEALED
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// ── 1a. Sealed CLASS — cannot be inherited ────────────────────────────
// Use when: you want to lock down a final implementation for
// security, performance, or design correctness.

public class Payment
{
    public virtual void Process(double amount)
        => Console.WriteLine($"[Payment]  Processing ${amount:F2}");
}

public sealed class CreditCardPayment : Payment
{
    public string CardNumber { get; set; }

    public CreditCardPayment(string cardNumber)
        => CardNumber = cardNumber;

    public override void Process(double amount)
        => Console.WriteLine($"[CreditCard {CardNumber[-4..]}] Charged ${amount:F2}");
    //                                      ↑ last 4 digits only

    public void ValidateCard()
        => Console.WriteLine($"[CreditCard] Card {CardNumber[-4..]} is valid.");
}

// ❌ This would be a compile error:
// public class PremiumCreditCard : CreditCardPayment { }
// → Cannot inherit from sealed class 'CreditCardPayment'

// ── 1b. Sealed METHOD — stop a specific override in a chain ──────────
// A derived class seals an overridden method so no further
// subclass can override it again.

public class Animal
{
    public virtual void Speak()
        => Console.WriteLine("[Animal]  Some sound..");
}

public class Dog : Animal
{
    public override void Speak()
        => Console.WriteLine("[Dog]     Woof!");

    public virtual void Fetch()
        => Console.WriteLine("[Dog]     Fetching the ball!");
}

public class Labrador : Dog
{
    // sealed here — Labrador locks down Speak(), no further override allowed
    public sealed override void Speak()
        => Console.WriteLine("[Labrador] Friendly woof!");

    public sealed override void Fetch()
        => Console.WriteLine("[Labrador] Enthusiastically fetching!");
}

public class GuideDog : Labrador
{
    // ✅ Can still add new methods
    public void Guide()
        => Console.WriteLine("[GuideDog] Guiding owner safely.");

    // ❌ These would be compile errors:
    // public override void Speak() { }   // sealed in Labrador
    // public override void Fetch() { }   // sealed in Labrador
}

// ── 1c. Sealed + IDisposable — real world usage ───────────────────────
// Many BCL types are sealed: string, decimal, DateTime are all sealed.
// Sealing avoids unintended behavior from subclassing critical types.


// 🔒 Why sealed?
// Prevent subclassing: An API key is a sensitive, security-critical type. Allowing inheritance could lead to misuse (e.g., overriding Dispose incorrectly or exposing the secret).

// Predictable behavior: Consumers of ApiKey know exactly how it works—no surprises from derived classes.

// Performance: The JIT can optimize sealed classes better since it doesn’t need to account for polymorphism.

// 🗑️ Why IDisposable?
// Explicit cleanup: Secrets should not linger in memory longer than necessary. By implementing Dispose, you give developers a clear way to wipe the key.

// Deterministic release: Instead of waiting for GC, you can immediately null out the string reference and mark the object as disposed.

// Integration with using: Developers can safely wrap ApiKey in a using block, ensuring cleanup even if exceptions occur.

// using (var apiKey = new ApiKey("ABCD1234SECRET"))
// {
//     Console.WriteLine(apiKey.MaskedKey); 
//     // Output: ABCD**********
// }
// At this point, Dispose() has run:
// "[ApiKey] Secret key wiped from memory."

public sealed class ApiKey : IDisposable
{
    private string? _key;
    private bool    _disposed = false;

    public ApiKey(string key) => _key = key;

    public string MaskedKey
        => _disposed ? "***DISPOSED***"
                     : $"{_key![..4]}{"*".PadRight(_key.Length - 4, '*')}";

    public void Dispose()
    {
        if (_disposed) return;
        _key     = null;
        _disposed = true;
        Console.WriteLine("[ApiKey] Secret key wiped from memory.");
    }
}


// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// PART 2 — STATIC
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// ── 2a. Static CLASS — pure utility, no instance needed ───────────────
// Use for: helper/utility functions, extension methods, constants.
// Cannot be instantiated or inherited.

public static class MathHelper
{
    public const double PI = 3.14159265358979; // constant — belongs to type

    public static double CircleArea(double radius)
        => PI * radius * radius;

    public static double Clamp(double value, double min, double max)
        => Math.Max(min, Math.Min(max, value));

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }
}

// ── 2b. Static MEMBERS inside a non-static class ─────────────────────
// Static members are shared across ALL instances.
// Instance members are unique per object.

public class BankAccount
{
    // Shared across ALL accounts — one value for the whole class
    private static int    _totalAccounts = 0;
    private static double _totalDeposits = 0;

    // Unique per instance
    public int    AccountId { get; }
    public string Owner     { get; }
    public double Balance   { get; private set; }

    public BankAccount(string owner, double initialDeposit)
    {
        _totalAccounts++;
        AccountId       = _totalAccounts;
        Owner           = owner;
        Balance         = initialDeposit;
        _totalDeposits += initialDeposit;

        Console.WriteLine($"[Account #{AccountId:D3}] Opened for {Owner} with ${initialDeposit:F2}");
    }

    public void Deposit(double amount)
    {
        Balance        += amount;
        _totalDeposits += amount;
        Console.WriteLine($"[Account #{AccountId:D3}] Deposited ${amount:F2} | Balance: ${Balance:F2}");
    }

    // Static method — belongs to the class, not any instance
    public static void PrintStats()
    {
        Console.WriteLine($"[Bank Stats] Total Accounts: {_totalAccounts} | Total Deposits: ${_totalDeposits:F2}");
    }
}

// ── 2c. Static CONSTRUCTOR — runs once before first use ───────────────
// Use to initialize static fields that need logic (not just a value).
// You cannot call it — CLR calls it automatically, exactly once.

public class AppConfig
{
    public static string  Environment { get; }
    public static string  DbConnection { get; }
    public static bool    IsProduction { get; }

    // No access modifier allowed — always private by nature
    static AppConfig()
    {
        Environment  = System.Environment.GetEnvironmentVariable("APP_ENV") ?? "Development";
        IsProduction = Environment == "Production";
        DbConnection = IsProduction
            ? "Server=prod-db;Database=shop"
            : "Server=localhost;Database=shop_dev";

        Console.WriteLine($"[AppConfig] Static constructor ran — Env: {Environment}");
    }

    // Prevent instantiation alongside static data
    private AppConfig() { }
}

// ── 2d. Extension METHODS — static class extending existing types ──────
// Must live in a static class. Add methods to types you don't own.

public static class StringExtensions
{
    public static bool   IsNullOrEmpty(this string? s)  => string.IsNullOrEmpty(s);
    public static string Truncate(this string s, int max)
        => s.Length <= max ? s : s[..max] + "...";
    public static string ToTitleCase(this string s)
        => string.Join(" ", s.Split(' ')
                              .Select(w => w.Length > 0
                                  ? char.ToUpper(w[0]) + w[1..].ToLower()
                                  : w));
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// PROGRAM
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Program
{
    public static void Main()
    {
        // ── Sealed Class Demo ─────────────────────────────────────────
        Console.WriteLine("=== 1a. Sealed Class ===");
        var card = new CreditCardPayment("4111111111111234");
        card.Process(199.99);
        card.ValidateCard();

        Console.WriteLine();

        // ── Sealed Method Demo ────────────────────────────────────────
        Console.WriteLine("=== 1b. Sealed Method ===");
        Animal[]  animals = { new Animal(), new Dog(), new Labrador(), new GuideDog() };
        foreach (var a in animals)
            a.Speak(); // each calls its own version, GuideDog uses Labrador's sealed one

        Console.WriteLine();

        var guide = new GuideDog();
        guide.Fetch();  // sealed in Labrador
        guide.Guide();  // new method — still allowed

        Console.WriteLine();

        // ── Sealed + IDisposable Demo ─────────────────────────────────
        Console.WriteLine("=== 1c. Sealed + IDisposable ===");
        using var apiKey = new ApiKey("sk-abcdef789xyz");
        Console.WriteLine($"Masked key: {apiKey.MaskedKey}");
        // Dispose() called here — key wiped
        Console.WriteLine($"After dispose: {apiKey.MaskedKey}");

        Console.WriteLine();

        // ── Static Class Demo ─────────────────────────────────────────
        Console.WriteLine("=== 2a. Static Class ===");
        Console.WriteLine($"Circle area (r=5):  {MathHelper.CircleArea(5):F4}");
        Console.WriteLine($"Clamp(150, 0, 100): {MathHelper.Clamp(150, 0, 100)}");
        Console.WriteLine($"IsPrime(17):        {MathHelper.IsPrime(17)}");
        Console.WriteLine($"IsPrime(18):        {MathHelper.IsPrime(18)}");

        // ❌ new MathHelper(); → Compile error: cannot create instance of static class

        Console.WriteLine();

        // ── Static Members Demo ───────────────────────────────────────
        Console.WriteLine("=== 2b. Static Members ===");
        var acc1 = new BankAccount("Alice", 5000);
        var acc2 = new BankAccount("Bob",   3000);
        var acc3 = new BankAccount("Carol", 7000);

        acc1.Deposit(1000);
        acc2.Deposit(500);

        BankAccount.PrintStats(); // called on TYPE, not instance

        Console.WriteLine();

        // ── Static Constructor Demo ───────────────────────────────────
        Console.WriteLine("=== 2c. Static Constructor ===");
        Console.WriteLine($"Environment:  {AppConfig.Environment}");
        Console.WriteLine($"DB:           {AppConfig.DbConnection}");
        Console.WriteLine($"IsProduction: {AppConfig.IsProduction}");
        // Static constructor already ran on first access above — won't run again

        Console.WriteLine();

        // ── Extension Methods Demo ────────────────────────────────────
        Console.WriteLine("=== 2d. Extension Methods ===");
        string title = "the quick brown fox";
        string long_text  = "This is a very long description that should be cut short";

        Console.WriteLine($"TitleCase:  {title.ToTitleCase()}");
        Console.WriteLine($"Truncate:   {long_text.Truncate(20)}");
        Console.WriteLine($"IsNullOrEmpty(\"\"):    {"".IsNullOrEmpty()}");
        Console.WriteLine($"IsNullOrEmpty(\"hi\"): {"hi".IsNullOrEmpty()}");
    }
}