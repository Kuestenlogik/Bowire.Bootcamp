// OrderStatus — the passive third hop. A WebSocket endpoint at
// /orders/stream pushes { orderId, status } frames as fictional
// orders move through "received" → "confirmed" → "shipped". The
// capstone uses this to prove that the WebSocket stream is well-formed
// throughout the diagnosis; the seam isn't here.
//
// Reachable from Bowire via `ws://localhost:5303/orders/stream`. The
// WebSocket plugin attaches and surfaces a single subscription called
// `orders/stream` in Discover.

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.Map("/orders/stream", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var ct = context.RequestAborted;

    // Emit a deterministic loop of orders, each moving through the
    // three states. One frame per 500 ms keeps the recording compact
    // but visible — 30 seconds of subscription = ~60 frames.
    var states = new[] { "received", "confirmed", "shipped" };
    var i = 0;
    while (!ct.IsCancellationRequested && socket.State == WebSocketState.Open)
    {
        var frame = JsonSerializer.SerializeToUtf8Bytes(new
        {
            orderId = $"ORD-{(i / 3) % 1000:D4}",
            status = states[i % states.Length],
            ts = DateTimeOffset.UtcNow.ToString("O"),
        });
        try
        {
            await socket.SendAsync(frame, WebSocketMessageType.Text, true, ct);
        }
        catch (OperationCanceledException) { break; }
        catch (WebSocketException) { break; }
        await Task.Delay(500, ct);
        i++;
    }
});

app.MapGet("/", () => Results.Text(
    "OrderStatus — WebSocket at /orders/stream. Frames every 500 ms.",
    "text/plain"));

app.Run();
