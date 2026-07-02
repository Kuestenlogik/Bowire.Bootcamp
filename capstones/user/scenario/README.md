# User capstone — scenario stack (Harbor Control Center)

Three small .NET services that, run together, reproduce a **flaky berthing** scenario in the **Harbor Control Center** domain (the same `Ship` / `Dock` / `Crane` / `PortCall` model as [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples)). Each is its own minimal `Microsoft.NET.Sdk.Web` project; nothing references Bowire (the User audience drives Bowire as an operator — none of the stack-under-diagnosis embeds the workbench).

## Layout

| Folder | Wire | Port | What it serves |
| --- | --- | --- | --- |
| `Berthing/` | REST + OpenAPI | `5301` | `POST /api/berth` accepts a berth request for a ship, calls `CraneOps.Reserve`, returns `{ portCallId, status }`. |
| `CraneOps/` | gRPC + Server Reflection | `5302` | `crane.CraneOps/Reserve` books a crane for a dock. Returns `RESOURCE_EXHAUSTED` on roughly 30 % of calls after a 200–500 ms stall (crane pool exhausted). |
| `PortCallStatus/` | WebSocket | `5303` | `ws://localhost:5303/portcalls/stream` pushes `{ portCallId, status }` events as port calls move through `arrived → berthed → unloading → departed`. |

## Run

Three terminals:

```bash
dotnet run --project capstones/user/scenario/Berthing        --urls http://localhost:5301
dotnet run --project capstones/user/scenario/CraneOps        --urls http://localhost:5302
dotnet run --project capstones/user/scenario/PortCallStatus  --urls http://localhost:5303
```

## Chaos config

The flakiness lives inside `CraneOps` — see the `// CHAOS:` comments in `CraneOps/Program.cs`. The defaults:

- 30 % of `Reserve` calls fail with `RESOURCE_EXHAUSTED`.
- Every failure pauses 200–500 ms before responding (the operator sees a duration spike paired with the error).
- Every success completes in 5–10 ms.

You can change the numbers in `Program.cs` for a stronger or weaker signal. The capstone is calibrated for the defaults.

> The numbers match the `bowire mock --chaos "latency:100-500,fail-rate:0.30"` overlay from the lab's optional CLI repro. That's the point — the live host's chaos profile is reproducible in the mock with the same dial.

## Why three separate processes?

The capstone is about diagnosing *where* in a multi-hop stack the failure sits. A single process would collapse the diagnosis into "the one thing is broken"; the whole point is that the operator walks *between* hops, comparing recording steps across sources, to localise the seam (here: upstream in `CraneOps.Reserve`, not in `Berthing`, not in the well-formed WebSocket stream).

## What's NOT in the scenario

- Authentication, TLS, service-discovery — every URL is plain `http://` / `ws://` on `localhost`.
- A database — `CraneOps` keeps crane state in memory.
- A frontend — the WebSocket is consumable from the Bowire workbench directly.

The point is the diagnosis loop, not the realism of the stack.

---

**Back to:** [User capstone](../README.md)
