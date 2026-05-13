```csharp
namespace Code;

// INHERITANCE: A class (child) can acquire the fields, properties, and methods
// of another class (parent) — promoting code reuse and establishing
// an "IS-A" relationship between types.

// ── Base Class (Parent) ───────────────────────────────────────────────────────
// Contains shared state and behavior common to ALL employees

public class Employee
{
    public string Name   { get; set; }
    public int    Id     { get; set; }
    public double Salary { get; set; }

    public Employee(string name, int id, double salary)
    {
        Name   = name;
        Id     = id;
        Salary = salary;
    }

    public void DisplayInfo()
        => Console.WriteLine($"[ID: {Id:D3}] {Name,-15} | Base Salary: ${Salary:F2}");

    // virtual — subclasses can override this with their own bonus logic
    public virtual double CalculateBonus()
        => Salary * 0.10;  // default: 10% bonus for all employees
}

// ── Derived Classes (Children) ────────────────────────────────────────────────
// Each inherits shared behavior, overrides only what differs

public class Manager : Employee
{
    public int TeamSize { get; set; }

    public Manager(string name, int id, double salary, int teamSize)
        : base(name, id, salary)           // calls Employee constructor
        => TeamSize = teamSize;

    // Managers get a bigger bonus based on team size
    public override double CalculateBonus()
        => Salary * 0.20 + (TeamSize * 500);

    public void ConductMeeting()
        => Console.WriteLine($"{Name} is conducting a team meeting with {TeamSize} members.");
}

public class Developer : Employee
{
    public string PrimaryLanguage { get; set; }

    public Developer(string name, int id, double salary, string language)
        : base(name, id, salary)
        => PrimaryLanguage = language;

    // Developers get a flat 15% bonus
    public override double CalculateBonus()
        => Salary * 0.15;

    public void WriteCode()
        => Console.WriteLine($"{Name} is writing {PrimaryLanguage} code..");
}

public class Intern : Employee
{
    public string University { get; set; }

    public Intern(string name, int id, double salary, string university)
        : base(name, id, salary)
        => University = university;

    // Interns get no bonus — base returns 10%, override to 0
    public override double CalculateBonus()
        => 0;

    public void Learn()
        => Console.WriteLine($"{Name} from {University} is learning the codebase..");
}

// ── Multi-Level Inheritance ───────────────────────────────────────────────────
// SeniorDeveloper IS-A Developer IS-A Employee — three levels deep

public class SeniorDeveloper : Developer
{
    public int YearsOfExperience { get; set; }

    public SeniorDeveloper(string name, int id, double salary, string language, int years)
        : base(name, id, salary, language)
        => YearsOfExperience = years;

    // Senior devs earn extra based on experience
    public override double CalculateBonus()
        => base.CalculateBonus() + (YearsOfExperience * 200);  // base 15% + experience

    public void MentorJuniors()
        => Console.WriteLine($"{Name} ({YearsOfExperience} yrs exp) is mentoring junior devs..");
}

public class Program
{
    public static void Main()
    {
        // ── Individual Behavior Demo ─────────────────────────────────────
        Console.WriteLine("=== Individual Behaviors ===");

        var manager = new Manager("Alice",   101, 90000, 8);
        var dev     = new Developer("Bob",   102, 75000, "C#");
        var intern  = new Intern("Charlie",  103, 25000, "MIT");
        var senior  = new SeniorDeveloper("Diana", 104, 95000, "C#", 10);

        manager.ConductMeeting();
        dev.WriteCode();
        intern.Learn();
        senior.MentorJuniors();

        Console.WriteLine();

        // ── Inherited Shared Behavior ─────────────────────────────────────
        Console.WriteLine("=== Employee Info (Inherited from Employee) ===");
        manager.DisplayInfo();   // inherited from Employee
        dev.DisplayInfo();       // inherited from Employee
        intern.DisplayInfo();    // inherited from Employee
        senior.DisplayInfo();    // inherited through Developer → Employee

        Console.WriteLine();

        // ── Polymorphic Bonus Calculation ─────────────────────────────────
        // All treated as Employee — each calls its own overridden CalculateBonus()
        Console.WriteLine("=== Bonus Calculation (Overridden per type) ===");
        Employee[] employees = { manager, dev, intern, senior };

        foreach (var emp in employees)
            Console.WriteLine($"{emp.Name,-15} | Bonus: ${emp.CalculateBonus():F2}");

        Console.WriteLine();

        // ── IS-A Relationship Check ───────────────────────────────────────
        Console.WriteLine("=== IS-A Relationship ===");
        Console.WriteLine($"manager is Employee?        {manager is Employee}");   // true
        Console.WriteLine($"senior  is Developer?       {senior  is Developer}");  // true
        Console.WriteLine($"senior  is Employee?        {senior  is Employee}");   // true
        Console.WriteLine($"intern  is Manager?         {intern  is Manager}");    // false
    }
}
```

**What this teaches:**

- **`base()`** constructor call shows how child classes pass values up the chain cleanly
- **`virtual` / `override`** — each subtype customizes `CalculateBonus()` without touching the parent
- **Multi-level inheritance** — `SeniorDeveloper → Developer → Employee` shows three levels, and `base.CalculateBonus()` reuses the parent's logic instead of duplicating it
- **Polymorphic `Employee[]` array** — all four types treated uniformly, each calling its own bonus logic at runtime (ties back to Polymorphism)
- **`is` keyword demo** — makes the IS-A relationship tangible and verifiable at runtime
- **Role-specific methods** (`ConductMeeting`, `WriteCode`, `Learn`, `MentorJuniors`) show that child classes extend the parent, not just override it