namespace OOP;

// OPEN/CLOSED PRINCIPLE (OCP):
// A class should be OPEN for extension but CLOSED for modification.
// Add new behavior by adding new code — not by changing existing, tested code.

// ── ❌ BAD: Adding new discount types forces modifying existing class ──────────

public class BadDiscountService
{
    public double ApplyDiscount(string customerType, double price)
    {
        if (customerType == "Regular")
            return price * 0.90;       // 10% off
        else if (customerType == "Premium")
            return price * 0.80;       // 20% off

        // ☠ Every new customer type = modify this method
        // ☠ Risk of breaking existing logic with each change
        // ☠ Violates OCP — this class is open for modification

        return price;
    }
}

// ── ✅ GOOD: Extend behavior by adding new classes, never touching old ones ────

// The contract — defines what every discount type must do
public interface IDiscountStrategy
{
    double ApplyDiscount(double price);
    string Description { get; }
}

// Each discount type is its own closed, independent class
public class NoDiscount : IDiscountStrategy
{
    public string Description => "No Discount";
    public double ApplyDiscount(double price) => price;
}

public class RegularDiscount : IDiscountStrategy
{
    public string Description => "Regular (10% off)";
    public double ApplyDiscount(double price) => price * 0.90;
}

public class PremiumDiscount : IDiscountStrategy
{
    public string Description => "Premium (20% off)";
    public double ApplyDiscount(double price) => price * 0.80;
}

// ✅ New requirement? Just ADD a new class — nothing existing is touched
public class SeasonalDiscount : IDiscountStrategy
{
    public string Description => "Seasonal (35% off)";
    public double ApplyDiscount(double price) => price * 0.65;
}

// Closed for modification — never needs to change when new discounts are added
public class PriceCalculator
{
    private readonly IDiscountStrategy _discount;

    public PriceCalculator(IDiscountStrategy discount)
        => _discount = discount;

    public void PrintFinalPrice(double price)
    {
        double final = _discount.ApplyDiscount(price);
        Console.WriteLine($"[{_discount.Description,-20}] ${price:F2} → ${final:F2}");
    }
}

public class Program
{
    public static void Main()
    {
        // ── OCP Violation Demo ───────────────────────────────────────────
        Console.WriteLine("=== ❌ BAD (No OCP) ===");
        var bad = new BadDiscountService();
        Console.WriteLine($"Regular  → ${bad.ApplyDiscount("Regular",  100):F2}");
        Console.WriteLine($"Premium  → ${bad.ApplyDiscount("Premium",  100):F2}");
        Console.WriteLine($"Unknown  → ${bad.ApplyDiscount("Unknown",  100):F2}");

        Console.WriteLine();

        // ── OCP Compliant Demo ───────────────────────────────────────────
        Console.WriteLine("=== ✅ GOOD (OCP Applied) ===");

        IDiscountStrategy[] strategies =
        {
            new NoDiscount(),
            new RegularDiscount(),
            new PremiumDiscount(),
            new SeasonalDiscount()   // ✅ Added with zero changes to existing code
        };

        foreach (var strategy in strategies)
            new PriceCalculator(strategy).PrintFinalPrice(100.00);

        // Adding a new discount type tomorrow?
        // → Create a new class implementing IDiscountStrategy
        // → PriceCalculator, existing strategies — completely untouched
    }
}


// **What this teaches:**

// - **Bad example** shows the classic `if/else` chain that must be modified every time — the textbook OCP violation
// - **Strategy Pattern** is the natural OCP solution — new behavior lives in new classes, not new conditions
// - **`SeasonalDiscount`** is the "new requirement" demo — added with literally zero changes to any existing code
// - **`PriceCalculator` stays closed** — it never needs to know about new discount types, it just uses the interface
// - **Formatted output** (`$100.00 → $65.00`) makes the before/after price transformation immediately readable