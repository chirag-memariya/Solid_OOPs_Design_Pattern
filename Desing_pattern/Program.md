**Quick interview cheat sheet:**

| Pattern | Category | One-line answer |
|---|---|---|
| **Singleton** | Creational | One instance, global access point |
| **Factory** | Creational | Delegate object creation to a factory |
| **Builder** | Creational | Build complex objects step by step |
| **Observer** | Behavioral | Subject notifies many subscribers on change |
| **Strategy** | Behavioral | Swap algorithms at runtime via interface |

**Key interview talking points per pattern:**
- **Singleton** — mention `lock` for thread safety, `??=` for lazy init, and why it can hurt testability
- **Factory** — mention it follows OCP (add new type = new class, not `if/else`)
- **Builder** — mention fluent interface (method chaining), great for optional parameters
- **Observer** — mention it's the foundation of all event systems and `INotifyPropertyChanged` in WPF/MAUI