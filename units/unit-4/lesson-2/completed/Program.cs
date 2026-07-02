// Lesson 4.2 — COMPLETED. The embedded host with the MCP adapter added
// (two lines, marked ← new). Discovered services are now exposed as MCP
// tools at /mcp. Verify with a tools/list POST to /mcp.

using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Protocol.Mcp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddBowire();
builder.Services.AddBowireMcpAdapter("http://localhost:5100");   // ← new: pin the discovery URL

var app = builder.Build();

app.MapOpenApi();
app.MapBowire();
app.MapBowireMcpAdapter("");                                     // ← new: mount /mcp at the root

app.MapGet("/hello/{name}", (string name) =>
    new { greeting = $"Hello, {name}!", receivedAt = DateTimeOffset.UtcNow });

app.Run();
