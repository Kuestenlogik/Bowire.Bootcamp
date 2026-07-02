# User capstone — reference solution

What's here:

| File | What it is |
| --- | --- |
| `berth-flake.bww` | Reference exported workspace. `bowire-workspace` v2 envelope — three pinned sources, three saved Compose tabs, one referenced recording (`flake-session-1`). |
| `RUNBOOK.md` | The full five-section diagnosis the User capstone asks for. |

## Using the reference

```bash
# Import the reference workspace into a fresh directory.
bowire workspace import berth-flake.bww --to ./imported-berth-flake

# Spin up the scenario stack (three terminals — see scenario/README.md).

# Open Bowire and the workspace pin will resolve the three sources.
bowire --port 5080
```

The recording itself (`flake-session-1.bwr`) is left to the operator to capture against their own scenario run — your laptop's random seed makes the failure pattern unique to your run, and the runbook step-id table reads more honestly when it's actually yours.

## Where the format definitions live

- `.bww` envelope shape — `src/Kuestenlogik.Bowire.Tool/Cli/WorkspaceCommand.cs` in main Bowire (`CanonicalFormatVersion = 2`).
- `.bwr` recording shape — captured by the workbench Recordings rail; `bowire recording validate <path>` checks one for structural integrity.
- `bowire mock --chaos` flag syntax — `src/Kuestenlogik.Bowire.Tool/Cli/BowireCli.cs` (`"latency:<min>-<max>,fail-rate:<0..1>"`).

---

**Back to:** [User capstone](../README.md)
