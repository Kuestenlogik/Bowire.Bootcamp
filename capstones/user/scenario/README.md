# User capstone — scenario stack

Three small .NET services that, run together, reproduce the "flaky checkout" scenario the capstone walks. Each one is its own minimal `Microsoft.NET.Sdk.Web` project; nothing references Bowire at all (the User audience drives Bowire as an operator — none of the stack-under-diagnosis embeds the workbench).

## Layout

| Folder | Wire | Port | What it serves |
| --- | --- | --- | --- |
| `Checkout/` | REST + OpenAPI | `5301` | `POST /api/checkout` accepts an order, calls `Inventory.Reserve`, returns `{ orderId, status }`. |
| `Inventory/` | gRPC + Server Reflection | `5302` | `inventory.Inventory/Reserve` reserves stock. Returns `RESOURCE_EXHAUSTED` on roughly 30 % of calls after a 200–500 ms stall. |
| `OrderStatus/` | WebSocket | `5303` | `ws://localhost:5303/orders/stream` pushes `{ orderId, status }` events as orders move through `received → confirmed → shipped`. |

## Run

Three terminals:

```bash
dotnet run --project capstones/user/scenario/Checkout    --urls http://localhost:5301
dotnet run --project capstones/user/scenario/Inventory   --urls http://localhost:5302
dotnet run --project capstones/user/scenario/OrderStatus --urls http://localhost:5303
```

## Chaos config

The flakiness lives inside `Inventory` — see the `// CHAOS:` comments in `Inventory/Program.cs`. The defaults are:

- 30 % of `Reserve` calls fail with `RESOURCE_EXHAUSTED`.
- Every failure pauses 200–500 ms before responding (the operator sees a duration spike paired with the error).
- Every success completes in 5–10 ms.

You can change the numbers in `Program.cs` if you want a stronger or weaker signal. The capstone is calibrated for the defaults.

> The numbers match the `bowire mock --chaos "latency:100-500,fail-rate:0.30"` overlay you use in Step 8 of the lab. That's the point — the live host's chaos profile is reproducible in the mock with the same dial.

## Why three separate processes?

The capstone is about diagnosing where in a multi-hop stack the failure sits. A single process would collapse the diagnosis into "the one thing is broken" — the whole point of the scenario is that the operator has to walk *between* hops, comparing recording steps across sources, to localise the seam.

## What's NOT in the scenario

- Authentication, TLS, service-discovery infrastructure — every URL is plain `http://` / `ws://` on `localhost`.
- A database — `Inventory` keeps stock in memory.
- A frontend — the WebSocket is consumable from the Bowire workbench directly; no React app required.

The point is the diagnosis loop, not the realism of the stack.

---

**Back to:** [User capstone](../README.md)
