# Lesson 0.3: Get Bowire running

> **Difficulty:** Beginner | **Duration:** 8 min | **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Overview

Before any course, you need a **workbench open in your browser**. This lesson gets you there with the least ceremony — pick one of the paths below. It's onboarding, not a deep dive: the full CLI story is [Unit 3](../../unit-3/README.md), the full embedded story is [Unit 4](../../unit-4/README.md). Every course assumes you've done this once.

## Fastest path — run a sample that already embeds Bowire

The Harbor **Combined** sample in [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples) *embeds Bowire*, so running it gives you a workbench with no install and no CLI:

```bash
# in a clone of Bowire.Samples
cd harbor-demo/src/Kuestenlogik.Bowire.Samples.Combined
dotnet run                      # → http://localhost:5101
```

Open **<http://localhost:5101/bowire>**. You have a workbench, already pointed at a multi-protocol service. This is all Unit 1 (the UI course) needs.

## Or — install the CLI and point it at any service

The standalone tool launches a workbench against any URL:

```bash
dotnet tool install --global Kuestenlogik.Bowire.Tool
bowire --version

# point it at a sample (the protocols/ demos don't embed Bowire):
#   cd Bowire.Samples/protocols/Rest.PetStore && dotnet run
bowire --url http://localhost:<port>          # opens http://localhost:5080
```

The CLI is the daily driver for the Integrator / DevOps / Admin course — [Unit 3](../../unit-3/README.md) covers it in full.

## Or — embed it in your own host

If you're building your own ASP.NET service, two lines mount the workbench inside it:

```csharp
builder.Services.AddBowire();
app.MapBowire();                 // workbench at /<host>/bowire
```

That's the Developer course's path — [Unit 4](../../unit-4/README.md) covers DI, auth, config and gating.

## Verify

Whichever path you picked, opening the workbench URL should render the rail strip on the left and a sidebar populated by auto-discovery. If it does, you're set. (No separate browser-client install — the workbench is served by whichever process you started.)

## Key Takeaways

1. **You need a running workbench before the courses** — pick the path that matches where you're headed.
2. **Fastest for the UI course:** run the embedded Harbor **Combined** sample → `:5101/bowire`, zero install.
3. **CLI** (`bowire --url`) and **embedded** (`AddBowire`/`MapBowire`) are covered in depth in Units 3 and 4.

## What's Next

**Continue:** → [Lesson 0.4: How this bootcamp works](../lesson-4/README.md)
