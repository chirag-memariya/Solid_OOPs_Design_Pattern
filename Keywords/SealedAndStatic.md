**Quick comparison for interviews:**

| | `sealed` | `static` |
|---|---|---|
| **Purpose** | Block inheritance | Block instantiation |
| **`new` keyword** | ✅ Allowed | ❌ Compile error |
| **Instance members** | ✅ Yes | ❌ Must all be static |
| **Constructor** | ✅ Normal constructor | Only static constructor |
| **Inheritance** | ❌ Cannot be subclassed | ❌ Cannot be subclassed |
| **Real examples** | `string`, `decimal`, `DateTime` | `Math`, `Console`, `Convert` |

**Key interview talking points:**
- **`sealed`** — improves JIT performance (compiler can devirtualize method calls), used heavily in BCL for security and correctness
- **`sealed` method** — stops a specific override mid-chain without sealing the whole class
- **`static` constructor** — runs exactly once, before any static member is accessed or instance is created, no access modifier allowed
- **`static` members** — shared state across all instances, useful for counters, caches, config — but be careful in multithreaded code
- **Extension methods** — must be in a `static` class, first param uses `this`, cannot access private members of the extended type