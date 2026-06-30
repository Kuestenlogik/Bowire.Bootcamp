// Inventory — the upstream gRPC service Checkout calls. This is where
// the seeded flakiness actually lives. Reserve returns RESOURCE_EXHAUSTED
// on ~30 % of calls after a 200–500 ms stall; the other ~70 % return
// a fresh reservation_id in 5–10 ms.
//
// The capstone operator finds the seam by lining up failed Checkout
// requests with failed Reserve steps in the recording — same wall-clock
// window, same SKU. That's the diagnostic move the runbook documents.

using Grpc.Core;
using Inventory.V1;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// HTTP/2 cleartext on every listener — same pattern as the TacticalApi
// sample in main Bowire. Plain http:// gRPC requires h2c prior-knowledge;
// Kestrel's Http1AndHttp2 default only upgrades via TLS + ALPN, so a
// fresh HTTP/1.1 connection never negotiates h2c.
builder.WebHost.ConfigureKestrel(o =>
{
    o.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http2);
});

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.MapGrpcService<ChaoticInventoryService>();
// Server Reflection — lets Bowire's generic gRPC plugin discover the
// Inventory service from a plain `grpc@http://localhost:5302` URL
// without needing the .proto bundled into the workbench.
app.MapGrpcReflectionService();

app.Run();

internal sealed class ChaoticInventoryService : Inventory.V1.Inventory.InventoryBase
{
    // CHAOS: the seeded failure rate. 0.30 = 30 % of Reserve calls
    // return RESOURCE_EXHAUSTED. Bump down to 0.0 for a happy path
    // (useful for sanity-checking the workbench wiring before the
    // diagnosis lab); bump up to 1.0 to see only failures.
    private const double FailRate = 0.30;

    // CHAOS: stall window before a failure responds. Successful calls
    // skip the stall and respond in 5–10 ms.
    private static readonly TimeSpan StallMin = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan StallMax = TimeSpan.FromMilliseconds(500);

    public override async Task<ReserveResponse> Reserve(
        ReserveRequest request,
        ServerCallContext context)
    {
        if (Random.Shared.NextDouble() < FailRate)
        {
            var stallMs = Random.Shared.Next(
                (int)StallMin.TotalMilliseconds,
                (int)StallMax.TotalMilliseconds);
            await Task.Delay(stallMs, context.CancellationToken);
            throw new RpcException(new Status(
                StatusCode.ResourceExhausted,
                $"stock exhausted for {request.Sku}"));
        }

        // Happy path: cheap and fast.
        await Task.Delay(Random.Shared.Next(5, 10), context.CancellationToken);
        return new ReserveResponse
        {
            ReservationId = Guid.NewGuid().ToString("N")[..12],
            Ok = true,
        };
    }
}
