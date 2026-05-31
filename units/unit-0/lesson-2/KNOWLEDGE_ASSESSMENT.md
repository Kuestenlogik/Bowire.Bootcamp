# Quiz: Lesson 0.2 — Setup

## Multiple Choice

### 1. What does `dotnet tool install --global Kuestenlogik.Bowire.Tool` actually install?

- [ ] A) Only the workbench host. Plugins must be installed separately.
- [ ] B) The workbench host plus every bundled protocol plugin in one tool.
- [ ] C) A NuGet feed configuration.
- [ ] D) A Docker image.

<details>
<summary>Answer</summary>

**B)** Single tool, ~50 MB, ships the workbench plus every bundled protocol plugin. No per-protocol opt-in; `bowire plugin list --bundled` shows the full list.

</details>

### 2. Where does Bowire keep user-installed plugins?

- [ ] A) Inside the global tool directory (`~/.dotnet/tools/...`)
- [ ] B) `~/.bowire/plugins/<package-id>/`
- [ ] C) The current working directory
- [ ] D) `/etc/bowire/plugins/`

<details>
<summary>Answer</summary>

**B)** `~/.bowire/plugins/<package-id>/`. Both NuGet-resolved .NET plugins and extracted sidecar zips land here, each under their own subdirectory.

</details>

### 3. How do you update a bundled plugin?

- [ ] A) `bowire plugin update <id>`
- [ ] B) `dotnet tool update -g Kuestenlogik.Bowire.Tool`
- [ ] C) Edit `~/.bowire/config.json`
- [ ] D) Bundled plugins can't be updated.

<details>
<summary>Answer</summary>

**B)** Bundled plugins update as part of the global tool. `bowire plugin update` is for user-installed plugins from `~/.bowire/plugins/`; bundled ones are pinned to the running tool version.

</details>

## True / False

### 4. Recordings are synced to a Küstenlogik cloud account.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Recordings live in `~/.bowire/recordings.json` on your machine. To share, check the file into git or paste it into Slack — there's no cloud sync.

</details>

### 5. Bowire phones home to send telemetry by default.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Bowire's outbound network calls are opt-in. The only network call enabled by default is the per-invocation discovery against the `--url` you supplied. The plugin-update check is opt-in via `--enable-update-check`.

</details>

## Short Answer

### 6. List the four user-state files / directories under `~/.bowire/` and what each holds.

<details>
<summary>Answer</summary>

- **`~/.bowire/plugins/`** — User-installed plugins (`bowire plugin install ...`).
- **`~/.bowire/recordings.json`** — Recording store the workbench's recorder writes to.
- **`~/.bowire/environments.json`** — Saved environments / variables.
- **`~/.bowire/secrets/`** — Encrypted secret store, used by `--auth-provider` plugins.
- (Bonus) **`~/.bowire/config.json`** — Per-user defaults.

</details>

## Score

- 6/6: Ready to point at a real API in Lesson 0.3.
- 4–5/6: Skim the troubleshooting table.
- < 4/6: Re-run the install hands-on.
