// CraneTelemetry gRPC service — server-streaming Watch + unary GetLatest.
// Used in the capstone to demo Bowire's streaming pane and the recording's
// ReceivedMessages shape.

using Grpc.Core;

namespace HarborTour.Grpc;

public sealed class CraneTelemetryService : CraneTelemetry.CraneTelemetryBase
{
    private static readonly string[] Statuses =
        ["idle", "lifting", "lowering", "maintenance"];

    public override Task<CraneReading> GetLatest(
        CraneRequest request, ServerCallContext context)
    {
        return Task.FromResult(SnapshotFor(request.CraneId, tick: 0));
    }

    public override async Task Watch(
        CraneWatchRequest request,
        IServerStreamWriter<CraneReading> responseStream,
        ServerCallContext context)
    {
        var count = Math.Clamp(request.Count == 0 ? 5 : request.Count, 1, 60);
        for (var i = 0; i < count && !context.CancellationToken.IsCancellationRequested; i++)
        {
            await responseStream.WriteAsync(SnapshotFor(request.CraneId, tick: i));
            try
            {
                await Task.Delay(1000, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    // Deterministic snapshots keyed on (craneId, tick) — every call with
    // the same arguments returns the same reading. That's what makes the
    // recording reproducible: replay through `bowire mock` re-emits the
    // captured response byte-for-byte, but a *fresh* recording against
    // this server also reproduces the same values, so coverage can be
    // re-captured without diff churn.
    private static CraneReading SnapshotFor(string craneId, int tick)
    {
        var status = Statuses[tick % Statuses.Length];
        var loadTons = status == "lifting" || status == "lowering"
            ? Math.Round(8.0 + (tick * 2.5) % 18.0, 1)
            : 0.0;
        var containerId = loadTons > 0
            ? $"MAEU{1000000 + (tick * 12345) % 8999999:0000000}"
            : string.Empty;
        // Frozen capturedAt instead of DateTimeOffset.UtcNow — keeps
        // recordings stable across re-captures (Lesson 5.1's frozen-
        // baseline strategy).
        var capturedAt = $"2026-06-01T08:00:{tick:D2}Z";
        return new CraneReading
        {
            CraneId = string.IsNullOrEmpty(craneId) ? "crane-01" : craneId,
            Status = status,
            LoadTons = loadTons,
            ContainerId = containerId,
            CapturedAt = capturedAt,
        };
    }
}
