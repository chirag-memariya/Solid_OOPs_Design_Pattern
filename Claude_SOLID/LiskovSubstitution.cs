namespace Code;

// LISKOV SUBSTITUTION PRINCIPLE (LSP):
// A subclass should be substitutable for its base class without breaking behavior.
// If S is a subtype of T, you should be able to use S wherever T is expected —
// and the program should still work correctly.

// ── ❌ BAD: Forcing Ostrich to inherit Fly() breaks substitution ──────────────

public class BadBird
{
    public virtual void Fly()
        => Console.WriteLine($"Bird is flying...");
}

public class BadSparrow : BadBird
{
    public override void Fly()
        => Console.WriteLine("Sparrow is flying...");
}

public class BadOstrich : BadBird
{
    public override void Fly()
        => throw new NotImplementedException("Ostriches can't fly!"); 
    // ☠ Caller expects Fly() to work on any BadBird
    // ☠ Passing BadOstrich where BadBird is expected → runtime crash
    // ☠ Subclass cannot be substituted — violates LSP
}

// ── ✅ GOOD: Separate contracts for flying and non-flying birds ───────────────

// Base contract — every bird has a name and can walk
public interface IBird
{
    string Name { get; set; }
    void Walk();
}

// Extended contract — only birds that can actually fly
public interface IFlyingBird : IBird
{
    void Fly();
}

// Sparrow fulfills both contracts honestly
public class Sparrow : IFlyingBird
{
    public string Name { get; set; } = "Sparrow";
    public void Walk() => Console.WriteLine($"{Name} is walking..");
    public void Fly()  => Console.WriteLine($"{Name} is flying..");
}

// Ostrich only fulfills IBird — no Fly() forced on it
public class Ostrich : IBird
{
    public string Name { get; set; } = "Ostrich";
    public void Walk() => Console.WriteLine($"{Name} is walking..");
}

// Eagle is a flying bird — substitutable wherever IFlyingBird is expected
public class Eagle : IFlyingBird
{
    public string Name { get; set; } = "Eagle";
    public void Walk() => Console.WriteLine($"{Name} is walking..");
    public void Fly()  => Console.WriteLine($"{Name} is soaring high..");
}

public class Program
{
    public static void Main()
    {
        // ── LSP Violation Demo ───────────────────────────────────────────
        Console.WriteLine("=== ❌ BAD (No LSP) ===");
        BadBird[] badBirds = { new BadSparrow(), new BadOstrich() };
        foreach (var bird in badBirds)
        {
            try   { bird.Fly(); }
            catch (NotImplementedException e)
            { Console.WriteLine($"[CRASH] {e.Message}"); } // ☠ runtime explosion
        }

        Console.WriteLine();

        // ── LSP Compliant Demo ───────────────────────────────────────────
        Console.WriteLine("=== ✅ GOOD (LSP Applied) ===");

        // All birds can walk — safely substitutable as IBird
        IBird[] allBirds = { new Sparrow(), new Ostrich(), new Eagle() };
        Console.WriteLine("-- All birds walk:");
        foreach (var bird in allBirds)
            bird.Walk();

        Console.WriteLine();

        // Only flying birds here — every substitution is safe and honest
        IFlyingBird[] flyingBirds = { new Sparrow(), new Eagle() };
        Console.WriteLine("-- Flying birds fly:");
        foreach (var bird in flyingBirds)
            bird.Fly();

        // Ostrich is never put in a flying context — no crashes, no lies
    }
}

// **What this teaches:**

// - **Bad example** shows the classic LSP trap — `Ostrich` inherits `Fly()` it can't fulfill, causing a runtime crash inside a loop that reasonably expects all `BadBird`s to fly
// - **`try/catch` in the bad demo** makes the crash visible and dramatic without stopping the program
// - **Interface segregation** (`IBird` + `IFlyingBird`) is the natural LSP fix — each type only promises what it can actually deliver
// - **`Eagle` added** as a second flying bird — reinforces that `IFlyingBird[]` is a safe, substitutable collection
// - **Two separate arrays** (`IBird[]` vs `IFlyingBird[]`) clearly show the two safe substitution contexts