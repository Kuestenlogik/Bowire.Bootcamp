// Lesson 4.3 — COMPLETED. The host from ../start/ with the interceptor
// mounted (one line, marked ← new). Every request from that point on is
// captured into the Intercept rail. Order matters: UseBowireInterceptor
// must sit before the routes you want observed.

using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Interceptor;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBowire();

var app = builder.Build();

app.UseBowireInterceptor();     // ← new: capture every request from here on

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapPost("/api/orders", (Order order) =>
    Results.Ok(new { orderId = Guid.NewGuid().ToString("N")[..8], order.Sku, status = "queued" }));

app.MapBowire("/bowire");

app.Run();

record Order(string Sku, int Qty);
