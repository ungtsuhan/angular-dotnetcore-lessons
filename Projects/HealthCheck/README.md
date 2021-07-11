# Health Check

Application with feature that reports the health of app infrastructure


## Chapter 2 Looking Around

### Creating .NET and Angular Project

Run `dotnet new angular -o HealthCheck`

### Perform a test run

hitting the **Run** button or the *F5* key

### Test StaticFile middleware

`app.UseStaticFiles()` middleware in `Startup.cs` allows application to serve static html

Create `test.html` in `wwwroot`

Run and browse route `/test.html`

### Delete unwanted components in Angular

Delete *counter* and *fetch data* components

### Create Sepeate App Routing Module

Create `app-routing.module.ts` in `src\app`

Import `app-routing.module.ts` in `app.module.ts`


## Chapter 3 Front-End and Back-End Interactions

### Adding HealthChecks middleware

Add following lines to the **ConfigureServices** method in `Startup.cs`

```cs
public void ConfigureServices(IServiceCollection services)
{
	services.AddHealthChecks();
}
```

Add following lines to the **Configure** Method

```cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
	endpoints.MapHealthChecks("/hc");
}
```
Press *F5* and browse route `/hc`

### Adding an ICMP check

Create an ICMPHealthCheck class

```cs
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    public class ICMPHealthCheck : IHealthCheck
    {
        private readonly string Host = "www.does-not-exist.com";
        private readonly int HealthyRoundtripTime = 300;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(Host);

                switch (reply.Status)
                {
                    case IPStatus.Success:
                        return (reply.RoundtripTime > HealthyRoundtripTime) ? 
                            HealthCheckResult.Degraded(): 
                            HealthCheckResult.Healthy();

                    default:
                        return HealthCheckResult.Unhealthy();
                }

            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy();
            }
        }
    }
}

```

### Adding the ICMPHealthCheck to the pipeline

Add `AddCheck` to the AddHealthChecks pipeline

```cs
public void ConfigureServices(IServiceCollection services)
{
	services.AddHealthChecks()
        .AddCheck<ICMPHealthCheck>("ICMP");
}
```