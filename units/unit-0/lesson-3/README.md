# Lesson 0.3: Hello Bowire

> **Difficulty:** Beginner | **Duration:** 5 min | **Prerequisites:** [Lesson 0.2](../lesson-2/README.md)

## Overview

Point your freshly-installed `bowire` at a public REST API, watch the workbench discover it, invoke one method. The smallest end-to-end loop that proves the install works — without writing any code or running any sample server yet.

You'll bring your own sample API in [Unit 1.1](../../unit-1/lesson-1/README.md); this lesson uses one that's already on the public web.

## Steps

### 1. Pick a target

We'll use the [Petstore reference API](https://petstore3.swagger.io/) — Swagger's canonical OpenAPI demo. It exposes a `/api/v3/openapi.json` document the REST plugin can discover.

### 2. Point Bowire at it

```bash
bowire --url https://petstore3.swagger.io/api/v3
```

The CLI:

- Boots the workbench UI on `http://localhost:5080/bowire`.
- Auto-opens your browser to it.
- Tells the workbench: "discover the server at `https://petstore3.swagger.io/api/v3`".

The REST plugin probes the conventional OpenAPI paths, finds `/api/v3/openapi.json`, parses it, and renders the operations as method nodes in the sidebar.

### 3. Explore the sidebar

The sidebar shows the Petstore's three tags (`pet`, `store`, `user`) as services, each containing its OpenAPI operations as methods:

```
🐕 pet
   └─ getPetById
   └─ addPet
   └─ updatePet
   └─ findPetsByStatus
   └─ findPetsByTags
   └─ deletePet
   └─ uploadFile
🛒 store
   └─ getInventory
   └─ placeOrder
   └─ getOrderById
   └─ deleteOrder
👤 user
   └─ ...
```

This is **auto-discovery** — Bowire didn't need a Postman collection, a `.proto`, or any per-operation configuration. The OpenAPI document the Petstore publishes is the source of truth, and the workbench rendered it.

### 4. Invoke `getPetById`

1. Click **pet → getPetById** in the sidebar.
2. The right pane shows the parameter form built from the OpenAPI schema. There's one parameter, `petId` (int64, required).
3. Enter `petId = 1`.
4. Click **Invoke**.

You should see a response like:

```json
{
  "id": 1,
  "category": { "id": 0, "name": "string" },
  "name": "doggie",
  "photoUrls": ["string"],
  "tags": [{"id": 0, "name": "string"}],
  "status": "available"
}
```

(The Petstore's demo data sometimes shows placeholder strings — what matters is that the *call worked*. Status code in the bottom-right of the response pane reads `200`.)

### 5. Try one streaming-style method (optional)

Petstore's REST endpoints are all unary. To see the streaming-pane shape, point Bowire at a public gRPC server with reflection enabled (your own service from Unit 1.2 will work too) — for now, just note that the **invoke pane shape is the same regardless of protocol**.

### 6. Stop the workbench

`Ctrl+C` in the terminal running `bowire`. The browser tab disconnects; nothing else to clean up.

## Troubleshooting

| Symptom | Fix |
|---|---|
| Browser doesn't auto-open | Manually visit `http://localhost:5080/bowire`. |
| Sidebar is empty / "No services discovered" | The REST plugin couldn't reach the OpenAPI document. Check the URL is exactly `https://petstore3.swagger.io/api/v3` (not `…/swagger`, not `…/v2`). |
| `Address already in use` | Port 5080 is taken. Stop whatever's holding it (`lsof -i :5080`) or run with `--port 5099`. |
| Response pane shows a CORS error | The browser blocked a cross-origin request. The CLI uses its own back-end-side HTTP client by default; CORS errors usually mean the browser was tricked into making the call client-side. Re-launch `bowire` cleanly. |

## Key Takeaways

1. **Install once, point at anything.** No per-target setup — the `--url` flag is the whole config.
2. **Auto-discovery is a property of the protocol plugin.** REST → OpenAPI probing; gRPC → reflection; GraphQL → introspection; MCP → handshake. Same `bowire --url` invocation regardless.
3. **The UI shape is constant across protocols.** Sidebar / invoke pane / response viewer don't change when you switch wires; only the underlying serialisation does.
4. **You don't need a sample server to start exploring.** Public schemas (Petstore, GitHub's GraphQL, public gRPC reflection demos, &c) are enough for first contact.

## What's Next

You're done with Unit 0. Time to bring up your own sample API and drive REST + gRPC side-by-side in the same workbench.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Unit 1: Workbench Basics](../../unit-1/README.md)

## Reference

- [Petstore reference API](https://petstore3.swagger.io/)
- [Bowire REST plugin docs](https://bowire.io/docs/protocols/rest.html)
- [Auto-discovery](https://bowire.io/docs/features/auto-discovery.html)
