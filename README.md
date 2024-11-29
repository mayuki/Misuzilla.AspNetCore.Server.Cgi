# Misuzilla.AspNetCore.Server.Cgi
Implementation of a CGI server adapter for ASP.NET Core.

This library will enable your ASP.NET Core application to be used with CGI.

> [!NOTE]
> This is a proof of concept, a toy, and is not intended for use in a production environment.

## How to use

Call `UseCGI` extension method of WebHost builder.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add: Enable CGI server implementation for ASP.NET Core.
builder.WebHost.UseCgi();

var app = builder.Build();

app.MapGet("/", () => "It works!");
app.Run();

```

## License
MIT License