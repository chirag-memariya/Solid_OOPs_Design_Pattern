Below is a **clean, production-style documentation** for the approach:
**“Hosting ASP.NET Core WebAPI inside an existing WPF or WCF application.”**

You can copy–paste this as internal technical documentation.

---

# 📄 **DOCUMENTATION**

## **Hosting ASP.NET Core WebAPI Inside a WPF/WCF Application**

---

# **1. Overview**

Many legacy systems use **WPF for management UI** and **WCF services** for backend operations.
Modern web applications require **REST/HTTP endpoints**, JSON responses, and browser-friendly APIs.

Instead of rewriting the entire WPF/WCF system, we can:

### ✔ Embed an ASP.NET Core WebAPI server **inside the existing WPF or WCF process**

### ✔ Expose REST endpoints without breaking the legacy architecture

### ✔ Keep all the old logic (recording, streaming, authentication, business logic)

This approach lets both systems work side-by-side:

```
+----------------------------------------------------------+
| Existing WPF/WCF Application                             |
|  - UI / Admin Logic                                      |
|  - Recording Logic                                       |
|                                                          |
|  +----------------------------------------------------+ |
|  | Embedded ASP.NET Core WebAPI (Kestrel)             | |
|  |  - Exposes REST endpoints                          | |
|  |  - JSON-based Web API for browsers                 | |
|  |  - Optional: SignalR / WebSocket event streaming   | |
|  +----------------------------------------------------+ |
+----------------------------------------------------------+
```

Web application now calls REST APIs while legacy WPF/WCF still runs normally.

---

# **2. Benefits**

### ✔ No need to rewrite WCF services

### ✔ No changes to existing WPF or WCF logic

### ✔ REST API available from the same executable

### ✔ Works on Windows (desktop or server)

### ✔ Supports SignalR, WebSockets, HTTPS

### ✔ Can slowly migrate features to REST without downtime

---

# **3. Requirements**

### ✔ .NET 6/7/8 installed

### ✔ WPF or WCF project using SDK-style csproj

### ✔ Add:

```xml
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

This enables ASP.NET Core hosting in desktop apps.

---

# **4. Project Setup**

## 4.1 Add Startup.cs

This defines routing, controllers, and middleware.

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        // Optional:
        // services.AddSignalR();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            // endpoints.MapHub<StreamHub>("/streamhub");
        });
    }
}
```

---

# **5. Create a WebAPI Controller**

Add a controller inside your WPF or WCF project.

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/record")]
public class RecordController : ControllerBase
{
    [HttpPost("start/{cameraId}")]
    public IActionResult Start(int cameraId)
    {
        RecordingManager.Start(cameraId); // existing code
        return Ok("Recording Started");
    }
}
```

Your WebApp can now call:

```
POST http://localhost:5005/api/record/start/5
```

---

# **6. Hosting WebAPI inside WPF**

Modify **App.xaml.cs**:

```csharp
public partial class App : Application
{
    private IHost? webHost;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        webHost = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://0.0.0.0:5005");
                webBuilder.UseStartup<Startup>();
            })
            .Build();

        webHost.Start();

        // Launch the WPF UI
        new MainWindow().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        webHost?.StopAsync().Wait();
        webHost?.Dispose();
        base.OnExit(e);
    }
}
```

### WebAPI is now hosted on:

```
http://localhost:5005/
```

---

# **7. Hosting WebAPI inside a WCF service**

For a self-hosted or Windows-service WCF application:

```csharp
public class RecordingWinService : ServiceBase
{
    private IHost? webHost;

    protected override void OnStart(string[] args)
    {
        webHost = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://0.0.0.0:5006");
                webBuilder.UseStartup<Startup>();
            })
            .Build();

        webHost.Start();

        // Start WCF logic
        RecordingWcfHost.Start();
    }

    protected override void OnStop()
    {
        webHost?.StopAsync().Wait();
        webHost?.Dispose();
    }
}
```

---

# **8. Architecture Summary**

### Existing system:

* WPF → Admin UI
* WCF → Recording server

### New system:

* Embedded WebAPI from same EXE
* WebApp connects to WPF/WCF backend via REST

**Flow:**

```
Web Browser (Angular/React)
       │
       ▼
REST API exposed from WPF/WCF (port 5005)
       │
       ▼
Existing WCF/WPF business logic
```

---

# **9. Additional Features**

### ✔ Enable HTTPS

Simply change:

```csharp
webBuilder.UseUrls("https://0.0.0.0:5005");
```

### ✔ Enable SignalR for live events / push notifications

```csharp
services.AddSignalR();
```

### ✔ Add WebSocket–based streaming

Use SignalR or a custom WebSocket middleware.

### ✔ Add Swagger UI

```csharp
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
```

---

# **10. Limitations**

| Feature         | Limitation                                             |
| --------------- | ------------------------------------------------------ |
| High load       | WPF process not ideal for many concurrent connections  |
| Video streaming | Should be delegated to a proper media server           |
| Linux hosting   | Not supported (WPF/WCF are Windows-only)               |
| Threading       | API requests run in Kestrel thread pool, not UI thread |

---

# **11. When to Use This Approach**

Use for:

✔ Internal enterprise tools
✔ Legacy WPF/WCF systems that need REST
✔ Transition to hybrid architecture
✔ Lightweight dashboards
✔ Recording control endpoints
✔ Status/health/ping APIs

Do NOT use for:

✘ Internet-scale traffic
✘ Heavy video distribution
✘ Large concurrent user bases

---

# **12. Conclusion**

This method allows you to modernize an existing WPF/WCF legacy application **without rewriting or migrating to a full backend**.

It is the fastest way to give your current system:

* REST WebAPI
* Browser-friendly endpoints
* SignalR/WebSocket support
* Future migration flexibility

All while the original application remains untouched.

---

If you want, I can also provide:

📌 **A complete sample project (folder structure + code)**
📌 **A version with WebAPI + SignalR + WCF combined**
📌 **A version with streaming preview snapshots to web**

Just tell me **“give sample project”**.
