
namespace Code;

// DISPOSE & FINALIZE — Resource Management in C#
//
// C# has a Garbage Collector (GC) that handles MANAGED memory automatically.
// But UNMANAGED resources (files, DB connections, sockets, streams) must be
// released manually — GC doesn't know public interface IAuthService
// {
//     Task<User?> ValidateUserAsync(string username, string password);
//     Task<string> GenerateJwtTokenAsync(User user);
// }

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await _userRepo.GetByUsernameAsync(username);
        if (user == null) return null;

        // Example: hash + compare
        if (!PasswordHasher.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<string> GenerateJwtTokenAsync(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"])
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim("role", user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
// how to clean those up.
//
// Two mechanisms exist for this:
//
// ┌─────────────┬──────────────────────────────────────────────────────┐
// │  Dispose()  │ Called EXPLICITLY by you (or `using` block)          │
// │             │ Deterministic — runs exactly when you want it        │
// │             │ Implement via IDisposable interface                   │
// ├─────────────┼──────────────────────────────────────────────────────┤
// │  Finalize() │ Called by GC AUTOMATICALLY when object is collected  │
// │  ~Destructor│ Non-deterministic — you don't control WHEN it runs   │
// │             │ Last safety net if Dispose() was never called        │
// └─────────────┴──────────────────────────────────────────────────────┘

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 1. BASIC IDisposable — simplest form
//    Use when your class holds unmanaged resources directly.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class FileLogger : IDisposable
{
    private StreamWriter? _writer;
    private bool _disposed = false; // guard against double-dispose

    public FileLogger(string filePath)
    {
        _writer = new StreamWriter(filePath, append: true);
        Console.WriteLine("[FileLogger] Opened file resource.");
    }

    public void Log(string message)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileLogger));

        _writer?.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        Console.WriteLine($"[FileLogger] Logged: {message}");
    }

    public void Dispose()
    {
        if (_disposed) return; // safe to call multiple times

        _writer?.Flush();
        _writer?.Close();
        _writer = null;
        _disposed = true;

        Console.WriteLine("[FileLogger] Disposed — file resource released.");
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 2. FULL DISPOSE PATTERN + FINALIZER
//    Use when your class holds BOTH managed and unmanaged resources.
//    Finalizer = last safety net if caller forgets to call Dispose().
//
//    The standard pattern:
//      Dispose()        → called by user/using → cleans everything
//      Dispose(true)    → cleans managed + unmanaged resources
//      Dispose(false)   → cleans ONLY unmanaged (called from finalizer)
//      ~Destructor      → called by GC → calls Dispose(false)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class DatabaseConnection : IDisposable
{
    private bool _disposed = false;

    // Simulated unmanaged resource (e.g. native DB handle)
    private IntPtr _nativeHandle;

    // Simulated managed resource (e.g. StreamWriter, another IDisposable)
    private StreamWriter? _auditLog;

    public string ConnectionString { get; }

    public DatabaseConnection(string connectionString)
    {
        ConnectionString = connectionString;
        _nativeHandle    = new IntPtr(42); // simulate acquiring native handle
        _auditLog        = new StreamWriter(Stream.Null); // simulate managed resource
        Console.WriteLine($"[DB] Connection opened: {connectionString}");
    }

    public void ExecuteQuery(string sql)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DatabaseConnection));

        Console.WriteLine($"[DB] Executing: {sql}");
    }

    // ── Public Dispose — called by user or `using` block ─────────────
    public void Dispose()
    {
        Dispose(disposing: true);

        // Tell GC: no need to call finalizer — we already cleaned up
        GC.SuppressFinalize(this);

        Console.WriteLine("[DB] GC.SuppressFinalize called — finalizer skipped.");
    }

    // ── Protected Dispose — the actual cleanup logic ──────────────────
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // ✅ Safe to clean MANAGED resources here
            // (only when called from Dispose(), not from finalizer)
            _auditLog?.Dispose();
            _auditLog = null;
            Console.WriteLine("[DB] Managed resources released.");
        }

        // ✅ Always clean UNMANAGED resources regardless of path
        if (_nativeHandle != IntPtr.Zero)
        {
            // In real code: release native handle via P/Invoke etc.
            _nativeHandle = IntPtr.Zero;
            Console.WriteLine("[DB] Unmanaged native handle released.");
        }

        _disposed = true;
    }

    // ── Finalizer — GC's last resort if Dispose() was never called ────
    // ⚠ Never call this directly — GC calls it automatically
    // ⚠ Only cleans unmanaged resources (managed may already be gone)
    ~DatabaseConnection()
    {
        Console.WriteLine("[DB] ⚠ Finalizer called by GC — Dispose() was missed!");
        Dispose(disposing: false); // unmanaged cleanup only
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 3. INHERITED DISPOSE PATTERN
//    Child class adds its own resources on top of parent's.
//    Override Dispose(bool) — never re-implement Dispose() itself.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class SecureConnection : DatabaseConnection
{
    private bool _disposed = false;
    private MemoryStream? _encryptionBuffer;

    public SecureConnection(string connectionString) : base(connectionString)
    {
        _encryptionBuffer = new MemoryStream();
        Console.WriteLine("[Secure] Encryption buffer allocated.");
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Clean child's managed resource
            _encryptionBuffer?.Dispose();
            _encryptionBuffer = null;
            Console.WriteLine("[Secure] Encryption buffer released.");
        }

        _disposed = true;

        // ✅ Always call base — parent cleans its own resources
        base.Dispose(disposing);
    }
}

public class Program
{
    public static void Main()
    {
        // ── 1. using block — Dispose() called automatically ──────────────
        Console.WriteLine("=== 1. using block (auto Dispose) ===");
        using (var logger = new FileLogger("log.txt"))
        {
            logger.Log("App started");
            logger.Log("User logged in");
        } // ← Dispose() called here automatically, even if exception thrown

        Console.WriteLine();

        // ── 2. using declaration (C# 8+) — cleaner syntax ────────────────
        Console.WriteLine("=== 2. using declaration (C# 8+) ===");
        using var db = new DatabaseConnection("Server=localhost;DB=shop");
        db.ExecuteQuery("SELECT * FROM Orders");
        db.ExecuteQuery("SELECT * FROM Users");
        // Dispose() called automatically at end of scope

        Console.WriteLine();

        // ── 3. Manual Dispose — you control when ─────────────────────────
        Console.WriteLine("=== 3. Manual Dispose ===");
        var conn = new DatabaseConnection("Server=localhost;DB=logs");
        conn.ExecuteQuery("INSERT INTO Logs VALUES ('test')");
        conn.Dispose(); // explicitly release resources

        // Calling Dispose() twice is safe — guarded by _disposed flag
        conn.Dispose(); // no crash, no double-release

        Console.WriteLine();

        // ── 4. Forgot to Dispose — Finalizer saves the day ───────────────
        Console.WriteLine("=== 4. Finalizer safety net ===");
        var forgotten = new DatabaseConnection("Server=localhost;DB=temp");
        forgotten.ExecuteQuery("SELECT 1");
        forgotten = null!; // simulate forgetting to dispose

        GC.Collect();           // force GC to run (demo only — never do in prod)
        GC.WaitForPendingFinalizers(); // wait for finalizer thread
        Console.WriteLine("[Main] GC collected the forgotten connection.");

        Console.WriteLine();

        // ── 5. Inherited Dispose ──────────────────────────────────────────
        Console.WriteLine("=== 5. Inherited Dispose ===");
        using var secure = new SecureConnection("Server=localhost;DB=secure");
        secure.ExecuteQuery("SELECT * FROM Secrets");
        // Dispose chain: SecureConnection.Dispose → DatabaseConnection.Dispose
    }
}


// **Key concepts explained:**

// **Why two methods exist:**

// | | `Dispose()` | `Finalize()` (~Destructor) |
// |---|---|---|
// | Called by | You / `using` block | GC automatically |
// | Timing | Deterministic — right when you want | Non-deterministic — whenever GC runs |
// | Cleans | Managed + unmanaged | Unmanaged only |
// | Performance | Fast | Slow — delays GC collection by one cycle |

// **The golden rules:**
// - Always use `using` block — it calls `Dispose()` even if an exception is thrown
// - Always guard with `_disposed = false` flag — prevents double-dispose crashes
// - Always call `GC.SuppressFinalize(this)` in `Dispose()` — skips the finalizer since cleanup already done, improves performance
// - Always call `base.Dispose(disposing)` in child classes — ensures the full chain cleans up
// - Never access managed resources inside the finalizer — they may already be collected by GC