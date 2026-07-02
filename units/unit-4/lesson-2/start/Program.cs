// Lesson 4.2 — START. The embedded host from Lesson 4.1 (workbench
// already mounted). Your task: expose the discovered services to an AI
// agent over one shared HTTP MCP endpoint. Diff against ../completed/.

using Kuestenlogik.Bowire;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddBowire();

var app = builder.Build();

app.MapOpenApi();
app.MapBowire();

app.MapGet("/hello/{name}", (string name) =>
    new { greeting = $"Hello, {name}!", receivedAt = DateTimeOffset.UtcNow });

app.Run();
