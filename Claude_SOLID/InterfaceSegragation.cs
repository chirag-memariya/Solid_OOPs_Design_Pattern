namespace Code;

// INTERFACE SEGREGATION PRINCIPLE (ISP):
// A class should not be forced to implement interfaces it does not use.
// Instead of one fat interface, create small focused ones —
// so each class only depends on what it actually needs.

// ── ❌ BAD: One bloated interface forces Robot to implement Eat() ─────────────

public interface IBadWorker
{
    void Work();
    void Eat();
    void Sleep();
    // ☠ Robot must implement Eat() and Sleep() — it has no stomach, no fatigue
    // ☠ Every new method added here breaks ALL implementing classes
    // ☠ One fat interface, many forced dependencies — violates ISP
}

public class BadHuman : IBadWorker
{
    public void Work()  => Console.WriteLine("[Human] Working...");
    public void Eat()   => Console.WriteLine("[Human] Eating...");
    public void Sleep() => Console.WriteLine("[Human] Sleeping...");
}

public class BadRobot : IBadWorker
{
    public void Work()  => Console.WriteLine("[Robot] Working...");
    public void Eat()   => throw new NotImplementedException("Robots don't eat!");  // ☠
    public void Sleep() => throw new NotImplementedException("Robots don't sleep!"); // ☠
}

// ── ✅ GOOD: Small focused interfaces — each class implements only what fits ───

// Segregated contracts — each captures one capability
public interface IWorkable
{
    void Work();
}

public interface IEatable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

// Human needs all three — implements all three honestly
public class Human : IWorkable, IEatable, ISleepable
{
    public string Name { get; set; } = "Human";
    public void Work()  => Console.WriteLine($"[{Name}]  Working 9 to 5..");
    public void Eat()   => Console.WriteLine($"[{Name}]  Having lunch..");
    public void Sleep() => Console.WriteLine($"[{Name}]  Sleeping 8 hours..");
}

// Robot only works — implements only what it needs, no forced stubs
public class Robot : IWorkable
{
    public string Name { get; set; } = "Robot";
    public void Work() => Console.WriteLine($"[{Name}]  Working 24/7..");
}

// Android is a robot that can also eat (charges via food port — why not!)
// ✅ Just add the interface — no existing code touched
public class Android : IWorkable, IEatable
{
    public string Name { get; set; } = "Android";
    public void Work() => Console.WriteLine($"[{Name}] Working at full capacity..");
    public void Eat()  => Console.WriteLine($"[{Name}] Consuming energy cells..");
}

public class Program
{
    public static void Main()
    {
        // ── ISP Violation Demo ───────────────────────────────────────────
        Console.WriteLine("=== ❌ BAD (No ISP) ===");
        IBadWorker human = new BadHuman();
        IBadWorker robot = new BadRobot();

        human.Work();
        human.Eat();

        robot.Work();
        try   { robot.Eat(); }
        catch (NotImplementedException e)
        { Console.WriteLine($"[CRASH] {e.Message}"); } // ☠ forced, useless method

        Console.WriteLine();

        // ── ISP Compliant Demo ───────────────────────────────────────────
        Console.WriteLine("=== ✅ GOOD (ISP Applied) ===");

        // Every worker can work — safe substitution
        IWorkable[] workers = { new Human(), new Robot(), new Android() };
        Console.WriteLine("-- All workers:");
        foreach (var w in workers)
            w.Work();

        Console.WriteLine();

        // Only things that eat — Robot never ends up here
        IEatable[] eaters = { new Human(), new Android() };
        Console.WriteLine("-- Things that eat:");
        foreach (var e in eaters)
            e.Eat();

        Console.WriteLine();

        // Only things that sleep — Robot and Android never end up here
        ISleepable[] sleepers = { new Human() };
        Console.WriteLine("-- Things that sleep:");
        foreach (var s in sleepers)
            s.Sleep();

        // Robot is never put in an eat/sleep context — no crashes, no lies
    }
}

// **What this teaches:**

// - **Bad example** mirrors the LSP crash pattern — `BadRobot` is forced to implement `Eat()` and `Sleep()` it can never fulfill, blowing up at runtime
// - **Three focused interfaces** (`IWorkable`, `IEatable`, `ISleepable`) replace the one bloated contract — each captures exactly one capability
// - **`Android` added** as a bonus — shows ISP's flexibility: mix and match interfaces freely without touching existing classes
// - **Three separate arrays** (`IWorkable[]`, `IEatable[]`, `ISleepable[]`) demonstrate that each interface is its own safe, substitutable context
// - **Robot never appears** in eat/sleep contexts — no stubs, no throws, no lies