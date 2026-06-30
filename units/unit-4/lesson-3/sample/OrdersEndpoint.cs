// Drop this fragment into samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs
// (or any host running app.UseBowireInterceptor()) to add a route that the
// Intercepted rail captures with a non-trivial request body.
//
// Placement: between any other Map* calls and app.Run() — after
// UseBowireInterceptor() in the pipeline so the flow is captured.
//
// Example call:
//   curl -X POST http://localhost:5181/api/orders \
//     -H "Content-Type: application/json" \
//     -d '{"sku":"WIDGET-001","qty":3}'

app.MapPost("/api/orders", (OrderCreate body) =>
    Results.Ok(new
    {
        orderId = Guid.NewGuid().ToString("N")[..8],
        body.Sku,
        body.Qty,
        status = "queued"
    }))
    .WithName("CreateOrder")
    .WithTags("Orders");

internal sealed record OrderCreate(string Sku, int Qty);
