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

Delete counter and fetch data components

### Create Sepeate App Routing Module

Create `app-routing.module.ts` in `src\app`

Import `app-routing.module.ts` in `app.module.ts`