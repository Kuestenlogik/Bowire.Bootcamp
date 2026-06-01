// HarborTour — Bowire Bootcamp capstone sample backend.
//
// A shipping-yard backend that exposes:
//   • REST  — container manifests at /api/containers/* (OpenAPI auto-published)
//   • gRPC  — live crane telemetry (server-streaming Watch + unary GetLatest)
//
// One process, two wires. The capstone walks you through pointing
// Bowire at this server, recording a representative session, replaying
// it as a mock, and wiring the whole thing into CI + an MCP agent.

using HarborTour;
using HarborTour.Grpc;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddGrpc();
// Server reflection lets Bowire's gRPC plugin walk the service list
// at discovery time without a .proto upload — same affordance Unit 1.2
// relied on.
builder.Services.AddGrpcReflection();
builder.Services.AddSingleton<ContainerManifestStore>();

var app = builder.Build();

app.MapOpenApi();

// ── REST: container manifests ────────────────────────────────────────

app.MapGet("/api/containers",
    (ContainerManifestStore store) => Results.Ok(store.ListSummaries()))
    .WithName("ListContainers")
    .WithSummary("List every container known to the harbour, summary view.");

app.MapGet("/api/containers/{id}",
    (string id, ContainerManifestStore store) =>
        store.TryGet(id, out var manifest)
            ? Results.Ok(manifest)
            : Results.NotFound(new { error = $"unknown container {id}" }))
    .WithName("GetContainer")
    .WithSummary("Full manifest for one container (ISO 6346 code).");

app.MapPost("/api/containers",
    ([FromBody] ContainerManifest manifest, ContainerManifestStore store) =>
    {
        store.Upsert(manifest);
        return Results.Created($"/api/containers/{manifest.Id}", manifest);
    })
    .WithName("UpsertContainer")
    .WithSummary("Add or update a container manifest in the harbour's registry.");

app.MapGet("/api/health",
    () => Results.Ok(new { status = "up", service = "HarborTour" }))
    .WithName("GetHealth")
    .WithSummary("Liveness probe — always returns OK.");

// ── gRPC: crane telemetry + reflection ────────────────────────────────

app.MapGrpcService<CraneTelemetryService>();
app.MapGrpcReflectionService();

app.Run("http://localhost:5101");
