// Tiny REST API — three endpoints, one OpenAPI document. Bowire reads
// the OpenAPI at discovery time and renders each operation as a
// form-driven method in the sidebar. No code-gen, no extra setup.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

// Conventional path. .NET 10's MapOpenApi defaults to
// /openapi/v1.json — Bowire's REST plugin probes the same path, so
// pointing `bowire --url http://localhost:5001` at the root is enough.
app.MapOpenApi();

app.MapGet("/hello/{name}", (string name) => new GreetingResponse(
    Greeting: $"Hello, {name}!",
    ReceivedAt: DateTimeOffset.UtcNow))
    .WithName("GetGreeting")
    .WithSummary("Greet someone by name.");

app.MapPost("/echo", (EchoRequest req) => req with { ReceivedAt = DateTimeOffset.UtcNow })
    .WithName("PostEcho")
    .WithSummary("Echo a message back, stamped with the server's receive time.");

app.MapGet("/health", () => Results.Ok(new { status = "up" }))
    .WithName("GetHealth")
    .WithSummary("Liveness probe — always returns OK.");

app.Run("http://localhost:5001");

public record GreetingResponse(string Greeting, DateTimeOffset ReceivedAt);
public record EchoRequest(string Message, DateTimeOffset? ReceivedAt = null);
