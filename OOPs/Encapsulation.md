Here's your cleaned-up, corrected, and improved version:

```csharp
namespace OOP;

// ENCAPSULATION: Bundling data (fields) and behavior (methods) into a single class,
// while restricting direct access to internal state.
// Access is controlled through properties and methods — not exposed directly.

public class BankAccount
{
    private double _balance;

    // Public read access, but only this class can write
    public double Balance
    {
        get => _balance;
        private set => _balance = value;
    }

    public void Deposit(double amount)
    {
        if (amount > 0)
            Balance += amount;
    }

    public void Withdraw(double amount)
    {
        if (amount > 0 && amount <= Balance)
            Balance -= amount;
    }

    public void PrintBalance()
    {
        Console.WriteLine($"Your Balance is: {_balance:C}");
    }
}

public class Program
{
    public static void Main()
    {
        // ── Encapsulation Demo ──────────────────────────
        BankAccount account = new BankAccount();

        account.Deposit(100);
        account.PrintBalance();        // Your Balance is: $100.00

        account.Withdraw(50);
        account.PrintBalance();        // Your Balance is: $50.00

        Console.WriteLine(account.Balance); // ✅ Controlled read access via property

        // account._balance = 100;     // ❌ Compile error — private field
        // account.Balance = 100;      // ❌ Compile error — private setter
    }
}
```

**Key fixes & improvements:**

- **Namespace** casing fixed: `oops` → `OOP` (PascalCase convention)
- **Stray `},`** at the end of `Main` removed
- **Expression-bodied properties** (`get => / set =>`) used for cleaner syntax
- **String interpolation** with `:C` format specifier added — prints balance as currency (e.g. `$50.00`)
- **Variable name** `bankAccount` → `account` (shorter, cleaner)
- **Comments** rewritten to be concise and accurate
- **Encapsulation demo comments** improved to show both `_balance` and `Balance` setter are protected