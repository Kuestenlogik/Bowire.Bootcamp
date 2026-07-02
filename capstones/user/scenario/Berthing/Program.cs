// Berthing — the flaky REST surface the capstone operator stares at.
//
// POST /api/berth accepts a berth request for an arriving ship, calls
// CraneOps.Reserve over gRPC to book a crane for the dock, and returns
// { portCallId, status }. When Reserve fails the berth request
// propagates a 502 — that's the misdirection the capstone is built
// around: Berthing *looks* flaky, but the seam sits upstream in CraneOps.

using Capstones.User.Berthing;
using Crane.V1;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton(_ =>
{
    // Plain HTTP/2 cleartext to CraneOps on localhost — gRPC over h2c
    // without TLS for laptop scenarios. Don't ship this shape to prod.
    var craneUrl = Environment.GetEnvironmentVariable("CRANE_URL")
        ?? "http://localhost:5302";
    var channel = GrpcChannel.ForAddress(craneUrl);
    return new CraneOps.CraneOpsClient(channel);
});

var app = builder.Build();
app.MapOpenApi();

app.MapPost("/api/berth", async (
    BerthRequest req,
    CraneOps.CraneOpsClient crane,
    CancellationToken ct) =>
{
    var portCallId = "PC-" + Guid.NewGuid().ToString("N")[..8];
    try
    {
        var reserve = await crane.ReserveAsync(new ReserveRequest
        {
            DockId = req.DockId,
            Cranes = req.Cranes <= 0 ? 1 : req.Cranes,
        }, cancellationToken: ct);
        return Results.Ok(new BerthResponse(portCallId, "berthed"));
    }
    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.ResourceExhausted)
    {
        // CraneOps said no crane — surface a 502 so the operator sees a
        // berth-shaped failure. The recording will show the matching
        // Reserve step in the same wall-clock window; that's the tell
        // that the seam is upstream.
        return Results.Json(new { error = "crane unavailable", portCallId }, statusCode: 502);
    }
})
.WithName("Berth")
.WithSummary("Request a berth for a ship. Calls CraneOps.Reserve under the hood; propagates a 502 when no crane can be booked.");

app.MapGet("/api/health", () => Results.Ok(new { status = "up", server = "berthing" }))
    .WithName("BerthingHealth")
    .WithSummary("Liveness probe.");

app.Run();

namespace Capstones.User.Berthing
{
    public record BerthRequest(string ShipId, string DockId, int Cranes);
    public record BerthResponse(string PortCallId, string Status);
}
