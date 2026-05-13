namespace Code;


// ╔══════════════════════════════════════════════════════════════════╗
// ║           DESIGN PATTERNS — Top 5 Interview Patterns            ║
// ║  1. Singleton      2. Factory     3. Builder                    ║
// ║  4. Observer       5. Strategy                                  ║
// ╚══════════════════════════════════════════════════════════════════╝

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 1. SINGLETON PATTERN
//    Ensure only ONE instance of a class exists across the entire app.
//    Use for: logging, config, database connection, caches.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Logger
{
    private static Logger? _instance;
    private static readonly object _lock = new object(); // thread-safe
    private int _logCount = 0;

    private Logger() { } // ❌ private — no one can do: new Logger()

    public static Logger Instance
    {
        get
        {
            lock (_lock) // double-checked locking for thread safety
            {
                _instance ??= new Logger();
                return _instance;
            }
        }
    }

    public void Log(string message)
    {
        _logCount++;
        Console.WriteLine($"[LOG #{_logCount:D3}] {message}");
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 2. FACTORY PATTERN
//    Let a factory method decide WHICH class to instantiate.
//    Caller asks for a product — factory handles the creation logic.
//    Use for: payment gateways, notification channels, parsers.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public interface IPayment
{
    void Pay(double amount);
}

public class CreditCardPayment : IPayment
{
    public void Pay(double amount)
        => Console.WriteLine($"[CreditCard] Paid ${amount:F2}");
}

public class PayPalPayment : IPayment
{
    public void Pay(double amount)
        => Console.WriteLine($"[PayPal]     Paid ${amount:F2}");
}

public class CryptoPayment : IPayment
{
    public void Pay(double amount)
        => Console.WriteLine($"[Crypto]     Paid ${amount:F2}");
}

public static class PaymentFactory
{
    // Caller never uses `new` — factory owns object creation
    public static IPayment Create(string type) => type switch
    {
        "creditcard" => new CreditCardPayment(),
        "paypal"     => new PayPalPayment(),
        "crypto"     => new CryptoPayment(),
        _            => throw new ArgumentException($"Unknown payment type: {type}")
    };
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 3. BUILDER PATTERN
//    Construct a complex object step by step.
//    Caller controls what gets set — nothing is mandatory by force.
//    Use for: query builders, HTTP requests, email composers, reports.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Email
{
    public string To      { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body    { get; set; } = "";
    public string Cc      { get; set; } = "";
    public bool   IsHtml  { get; set; } = false;

    public override string ToString()
        => $"[Email] To: {To} | CC: {Cc} | Subject: {Subject} | HTML: {IsHtml}\n         Body: {Body}";
}

public class EmailBuilder
{
    private readonly Email _email = new Email();

    public EmailBuilder To(string to)          { _email.To      = to;      return this; }
    public EmailBuilder Subject(string subject) { _email.Subject = subject; return this; }
    public EmailBuilder Body(string body)       { _email.Body    = body;    return this; }
    public EmailBuilder Cc(string cc)           { _email.Cc      = cc;      return this; }
    public EmailBuilder AsHtml()                { _email.IsHtml  = true;    return this; }

    public Email Build() => _email; // returns the fully constructed object
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 4. OBSERVER PATTERN
//    One subject notifies many observers when its state changes.
//    Observers subscribe/unsubscribe freely — subject doesn't know details.
//    Use for: event systems, notifications, stock prices, UI data binding.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public interface IObserver
{
    void Update(string stockName, double price);
}

public class StockMarket  // Subject
{
    private readonly List<IObserver> _observers = new();
    private double _price;
    public string StockName { get; }

    public StockMarket(string stockName) => StockName = stockName;

    public void Subscribe(IObserver observer)   => _observers.Add(observer);
    public void Unsubscribe(IObserver observer) => _observers.Remove(observer);

    public double Price
    {
        get => _price;
        set
        {
            _price = value;
            NotifyAll(); // automatically alerts all subscribers on change
        }
    }

    private void NotifyAll()
    {
        foreach (var observer in _observers)
            observer.Update(StockName, _price);
    }
}

public class PhoneAlert : IObserver
{
    public string Owner { get; }
    public PhoneAlert(string owner) => Owner = owner;
    public void Update(string stock, double price)
        => Console.WriteLine($"[📱 Phone  → {Owner,-6}] {stock} is now ${price:F2}");
}

public class EmailAlert : IObserver
{
    public string Owner { get; }
    public EmailAlert(string owner) => Owner = owner;
    public void Update(string stock, double price)
        => Console.WriteLine($"[📧 Email  → {Owner,-6}] {stock} is now ${price:F2}");
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 5. STRATEGY PATTERN
//    Define a family of algorithms, encapsulate each one,
//    and make them interchangeable at runtime.
//    Use for: sorting, compression, routing, pricing, auth.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public interface ISortStrategy
{
    void Sort(List<int> data);
    string Name { get; }
}

public class BubbleSort : ISortStrategy
{
    public string Name => "Bubble Sort";
    public void Sort(List<int> data)
    {
        // simple bubble sort for demo
        for (int i = 0; i < data.Count - 1; i++)
            for (int j = 0; j < data.Count - i - 1; j++)
                if (data[j] > data[j + 1])
                    (data[j], data[j + 1]) = (data[j + 1], data[j]);
    }
}

public class QuickSort : ISortStrategy
{
    public string Name => "Quick Sort";
    public void Sort(List<int> data) => data.Sort(); // built-in quicksort
}

public class Sorter  // Context — uses whatever strategy is injected
{
    private ISortStrategy _strategy;

    public Sorter(ISortStrategy strategy) => _strategy = strategy;

    // ✅ Swap strategy at runtime — no class change needed
    public void SetStrategy(ISortStrategy strategy) => _strategy = strategy;

    public void Sort(List<int> data)
    {
        Console.WriteLine($"[{_strategy.Name}] Sorting: {string.Join(", ", data)}");
        _strategy.Sort(data);
        Console.WriteLine($"[{_strategy.Name}] Result:  {string.Join(", ", data)}");
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// PROGRAM — Demo all 5 patterns
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Program
{
    public static void Main()
    {
        // ── 1. SINGLETON ─────────────────────────────────────────────────
        Console.WriteLine("=== 1. SINGLETON ===");
        var log1 = Logger.Instance;
        var log2 = Logger.Instance;
        log1.Log("App started");
        log2.Log("User logged in");
        Console.WriteLine($"Same instance? {ReferenceEquals(log1, log2)}"); // true

        Console.WriteLine();

        // ── 2. FACTORY ───────────────────────────────────────────────────
        Console.WriteLine("=== 2. FACTORY ===");
        string[] methods = { "creditcard", "paypal", "crypto" };
        foreach (var method in methods)
            PaymentFactory.Create(method).Pay(99.99);

        Console.WriteLine();

        // ── 3. BUILDER ───────────────────────────────────────────────────
        Console.WriteLine("=== 3. BUILDER ===");
        var simpleEmail = new EmailBuilder()
            .To("alice@example.com")
            .Subject("Hello!")
            .Body("How are you?")
            .Build();

        var richEmail = new EmailBuilder()
            .To("bob@example.com")
            .Cc("team@example.com")
            .Subject("Sprint Update")
            .Body("<h1>All tasks done!</h1>")
            .AsHtml()
            .Build();

        Console.WriteLine(simpleEmail);
        Console.WriteLine(richEmail);

        Console.WriteLine();

        // ── 4. OBSERVER ──────────────────────────────────────────────────
        Console.WriteLine("=== 4. OBSERVER ===");
        var stock = new StockMarket("AAPL");

        var alice = new PhoneAlert("Alice");
        var bob   = new EmailAlert("Bob");
        var carol = new PhoneAlert("Carol");

        stock.Subscribe(alice);
        stock.Subscribe(bob);
        stock.Subscribe(carol);

        stock.Price = 175.50;  // notifies all 3

        Console.WriteLine("-- Bob unsubscribes --");
        stock.Unsubscribe(bob);

        stock.Price = 182.00;  // notifies only Alice and Carol

        Console.WriteLine();

        // ── 5. STRATEGY ──────────────────────────────────────────────────
        Console.WriteLine("=== 5. STRATEGY ===");
        var data1 = new List<int> { 5, 3, 8, 1, 9, 2 };
        var data2 = new List<int> { 5, 3, 8, 1, 9, 2 };

        var sorter = new Sorter(new BubbleSort());
        sorter.Sort(data1);

        sorter.SetStrategy(new QuickSort()); // swap at runtime — no new Sorter needed
        sorter.Sort(data2);
    }
}