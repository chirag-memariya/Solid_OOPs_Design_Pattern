**Key concepts explained:**

**Why two methods exist:**

| | `Dispose()` | `Finalize()` (~Destructor) |
|---|---|---|
| Called by | You / `using` block | GC automatically |
| Timing | Deterministic — right when you want | Non-deterministic — whenever GC runs |
| Cleans | Managed + unmanaged | Unmanaged only |
| Performance | Fast | Slow — delays GC collection by one cycle |

**The golden rules:**
- Always use `using` block — it calls `Dispose()` even if an exception is thrown
- Always guard with `_disposed = false` flag — prevents double-dispose crashes
- Always call `GC.SuppressFinalize(this)` in `Dispose()` — skips the finalizer since cleanup already done, improves performance
- Always call `base.Dispose(disposing)` in child classes — ensures the full chain cleans up
- Never access managed resources inside the finalizer — they may already be collected by GC