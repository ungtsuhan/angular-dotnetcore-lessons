# Health Check

Application that reports the health of app infrastructure

## Chapter 2 Looking Around

### Creating .NET and Angular Project

- Run `dotnet new angular -o HealthCheck`

### Perform a test run

- hitting the **Run** button or the *F5* key

### Test StaticFile middleware

- `app.UseStaticFiles()` middleware in `Startup.cs` allows application to serve static html

-  `test.html` in `wwwroot`

- Run and browse route `/test.html`

### Delete unwanted components in Angular

- Delete *counter* and *fetch data* components

### Create Sepeate App Routing Module

- Create `app-routing.module.ts` in `src\app`

- Import `app-routing.module.ts` in `app.module.ts`


## Chapter 3 Front-End and Back-End Interactions

### Adding HealthChecks middleware

- Add following lines to the **ConfigureServices** method in `Startup.cs`

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

- Press *F5* and browse route `/hc`

### Adding an ICMP check

- Create an ICMPHealthCheck class

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

- Add `AddCheck` to the AddHealthChecks pipeline

```cs
public void ConfigureServices(IServiceCollection services)
{
	services.AddHealthChecks()
        .AddCheck<ICMPHealthCheck>("ICMP");
}
```

### Add paramters and response messages

- Add constructor in ICMPHealthCheck to accept *host* and *healthRoundtripTime* parameters

- Construct message and pass to HealthCheckResult function

- Update middleware as below

```cs
 services.AddHealthChecks()
    .AddCheck("ICMP_01", new ICMPHealthCheck("www.ryadel.com", 100))
    .AddCheck("ICMP_02", new ICMPHealthCheck("www.google.com", 100))
    .AddCheck("ICMP_03", new ICMPHealthCheck("www.does-not-exist.com", 100));
```

### Implementing a custom output message

Create CustomHealthCheckOptions.cs

```cs
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;

namespace HealthCheck
{
    public class CustomHealthCheckOptions: HealthCheckOptions
    {
        public CustomHealthCheckOptions(): base()
        {
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            ResponseWriter = async (c, r) =>
            {
                c.Response.ContentType = MediaTypeNames.Application.Json;
                c.Response.StatusCode = StatusCodes.Status200OK;

                var result = JsonSerializer.Serialize(new
                {
                    checks = r.Entries.Select(e => new 
                    {
                        name = e.Key,
                        responseTime = e.Value.Duration.TotalMilliseconds,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description
                    }),
                    totalStatus = r.Status,
                    totalResponseTime = r.TotalDuration.TotalMilliseconds,
                }, jsonSerializerOptions);

                await c.Response.WriteAsync(result);
            };

        }
    }
}
```

- Configure the output message by changing the following lines in *Configure* Method

```cs
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/hc", new CustomHealthCheckOptions());
});
```

- Example response as below:

```json
{
    "checks": [
        {
            "name": "ICMP_01",
            "responseTime": 137.1552,
            "status": "Healthy",
            "description": "ICMP to www.ryadel.com took 12 ms"
        },
        {
            "name": "ICMP_02",
            "responseTime": 174.807,
            "status": "Healthy",
            "description": "ICMP to www.google.com took 12 ms"
        },
        {
            "name": "ICMP_03",
            "responseTime": 5161.7416,
            "status": "Unhealthy",
            "description": "ICMP to www.does-not-exist.com failed: TimedOut"
        }
    ],
    "totalStatus": 0,
    "totalResponseTime": 5184.3754
}
```

### Create health-check component

- Run `ng g c health-check`

- ts code as below:

```ts
import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-health-check',
  templateUrl: './health-check.component.html',
  styleUrls: ['./health-check.component.css']
})
export class HealthCheckComponent implements OnInit {

  public result: Result;

  constructor(private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) { }

  ngOnInit(): void {
    this.http.get<Result>(this.baseUrl + 'hc').subscribe(result => {
      this.result = result;
    }, error => console.error(error))
  }

}

interface Result {
  checks: Check[],
  totalStatus: string,
  totalResponseTime: string
}

interface Check {
  name: string,
  status: string,
  responseTime: number
}

```

### Add routing for health check component

- Add below path in app-routing module

```
{ path: 'health-check', component: HealthCheckComponent }
```

### Add Tab to Navbar

- Add li to the tab list

```
<li class="nav-item" [routerLinkActive]="['link-active']">
    <a class="nav-link text-dark" [routerLink]="['/health-check']">Health Check</a>
</li>
```

### Populate result to table in html

- Add code below to health-check.component.html

```html
<h1>Health Check</h1>

<p>Here are the results of our health check:</p>

<p *ngIf="!result"><em>Loading...</em></p>

<table *ngIf="result">
  <thead>
    <tr>
      <th>Name</th>
      <th>Response Time</th>
      <th>Status</th>
      <th>Description</th>
    </tr>  
  </thead>
  <tr *ngFor="let check of result.checks">
    <td>{{ check.name }}</td>
    <td>{{ check.responseTime }}</td>
    <td>{{ check.status }} </td>
    <td>{{ check.description }}</td>  
  </tr>
</table>
```

- Apply css to table