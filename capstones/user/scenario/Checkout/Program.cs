// Checkout — the flaky REST surface the capstone operator stares at.
//
// POST /api/checkout accepts an order, calls Inventory.Reserve over
// gRPC, returns { orderId, status }. When Reserve fails the checkout
// propagates a 502 — that's the misdirection the capstone is built
// around: Checkout *looks* flaky, but the seam sits upstream.

using Capstones.User.Checkout;
using Grpc.Net.Client;
using Inventory.V1;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton(_ =>
{
    // Plain HTTP/2 cleartext to Inventory on localhost. The mirror of
    // the TacticalApi sample in main Bowire — gRPC over h2c without TLS
    // for laptop scenarios. Don't ship this shape to production.
    var inventoryUrl = Environment.GetEnvironmentVariable("INVENTORY_URL")
        ?? "http://localhost:5302";
    var channel = GrpcChannel.ForAddress(inventoryUrl);
    return new Inventory.V1.Inventory.InventoryClient(channel);
});

var app = builder.Build();
app.MapOpenApi();

app.MapPost("/api/checkout", async (
    CheckoutRequest req,
    Inventory.V1.Inventory.InventoryClient inventory,
    CancellationToken ct) =>
{
    var orderId = Guid.NewGuid().ToString("N")[..8];
    try
    {
        var reserve = await inventory.ReserveAsync(new ReserveRequest
        {
            Sku = req.Items[0].Sku,
            Quantity = req.Items[0].Qty
        }, cancellationToken: ct);
        return Results.Ok(new CheckoutResponse(orderId, "confirmed"));
    }
    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.ResourceExhausted)
    {
        // Inventory said no — surface a 502 so the operator sees a
        // checkout-shaped failure. The recording will show the
        // matching Reserve step in the same wall-clock window; that's
        // the tell that the seam is upstream.
        return Results.Json(new { error = "inventory unavailable", orderId }, statusCode: 502);
    }
})
.WithName("Checkout")
.WithSummary("Submit an order. Calls Inventory.Reserve under the hood; propagates a 502 when stock can't be reserved.");

app.MapGet("/api/health", () => Results.Ok(new { status = "up", server = "checkout" }))
    .WithName("CheckoutHealth")
    .WithSummary("Liveness probe.");

app.Run();

namespace Capstones.User.Checkout
{
    public record CheckoutRequest(CheckoutItem[] Items, string Buyer);
    public record CheckoutItem(string Sku, int Qty);
    public record CheckoutResponse(string OrderId, string Status);
}
