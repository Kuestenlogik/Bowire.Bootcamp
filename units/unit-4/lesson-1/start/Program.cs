// Lesson 4.1 — START. A bare ASP.NET host with one route and an OpenAPI
// document. Your task: embed the Bowire workbench in it (two lines).
// Diff this against ../completed/Program.cs to see exactly what changes.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

// Your service's own routes stay exactly where they are.
app.MapGet("/hello/{name}", (string name) =>
    new { greeting = $"Hello, {name}!", receivedAt = DateTimeOffset.UtcNow });

app.Run();
