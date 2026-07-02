// Lesson 5.1 — START. A protocol-plugin skeleton. It compiles and Bowire's
// assembly scan discovers it, but it surfaces no services yet. Your task:
//   TODO 1 — make DiscoverAsync advertise a "Pirate" service with one unary
//            method, "Translate".
//   TODO 2 — make InvokeAsync read { "text": "..." } and answer with
//            { "pirate": "..." }.
// Diff against ../completed/PirateProtocol.cs to see the finished shape.

using System.Runtime.CompilerServices;
using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Models;

namespace Bowire.Plugin.Pirate;

/// <summary>A toy protocol plugin that translates plain text into pirate speak.</summary>
public sealed class PirateProtocol : IBowireProtocol
{
    public string Name => "Pirate Speak";

    public string Id => "pirate";

    public string IconSvg =>
        """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 10h16"/><path d="M12 10v10"/><path d="M7 20h10"/><circle cx="12" cy="6" r="2"/></svg>""";

    // TODO 1: return one BowireServiceInfo named "Pirate" carrying a single
    // unary BowireMethodInfo "Translate" (input field "text", output field
    // "pirate"). Right now the sidebar stays empty.
    public Task<List<BowireServiceInfo>> DiscoverAsync(
        string serverUrl, bool showInternalServices, CancellationToken ct = default) =>
        Task.FromResult(new List<BowireServiceInfo>());

    // TODO 2: parse jsonMessages[0] as { "text": "..." }, translate it, and
    // return the pirate string as { "pirate": "..." }.
    public Task<InvokeResult> InvokeAsync(
        string serverUrl, string service, string method,
        List<string> jsonMessages, bool showInternalServices,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default) =>
        Task.FromResult(new InvokeResult(
            Response: """{ "todo": "implement InvokeAsync" }""",
            DurationMs: 0,
            Status: "not-implemented",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)));

    // Pirate Speak is unary-only — nothing to stream, no channel to open.
    public async IAsyncEnumerable<string> InvokeStreamAsync(
        string serverUrl, string service, string method,
        List<string> jsonMessages, bool showInternalServices,
        Dictionary<string, string>? metadata = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public Task<IBowireChannel?> OpenChannelAsync(
        string serverUrl, string service, string method,
        bool showInternalServices, Dictionary<string, string>? metadata = null,
        CancellationToken ct = default) =>
        Task.FromResult<IBowireChannel?>(null);
}
