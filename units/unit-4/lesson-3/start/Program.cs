// Lesson 4.3 — START. An embedded host with a couple of routes. Your
// task: capture every request the host serves in the Intercept rail by
// adding one middleware line. Diff against ../completed/.

using Kuestenlogik.Bowire;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBowire();

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapPost("/api/orders", (Order order) =>
    Results.Ok(new { orderId = Guid.NewGuid().ToString("N")[..8], order.Sku, status = "queued" }));

app.MapBowire("/bowire");

app.Run();

record Order(string Sku, int Qty);
