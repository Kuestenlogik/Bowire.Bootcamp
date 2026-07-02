// Lesson 4.1 — COMPLETED. The bare host from ../start/ with the Bowire
// workbench embedded. Two lines added (marked ← new); everything else is
// unchanged. Run it and open http://localhost:<port>/bowire.

using Kuestenlogik.Bowire;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddBowire();        // ← new: register the workbench + plugin host

var app = builder.Build();

app.MapOpenApi();
app.MapBowire();                     // ← new: mount the workbench at /bowire

// Your service's own routes stay exactly where they are.
app.MapGet("/hello/{name}", (string name) =>
    new { greeting = $"Hello, {name}!", receivedAt = DateTimeOffset.UtcNow });

app.Run();
