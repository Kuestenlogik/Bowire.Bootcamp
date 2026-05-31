// Drop-in replacement for the MyProtocol class the
// `dotnet new bowire-plugin` scaffold emits. Used in Lesson 05 as
// the "your turn — make the plugin actually do something" edit.
//
// Renames the demo service+method to a pirate theme and swaps the
// scaffold's echo Invoke for a tiny English-to-Pirate translator so
// users see their own code on the response pane.

using System.Runtime.CompilerServices;
using System.Text.Json;
using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Models;

namespace Bowire.Plugin.Pirate;

public sealed class PirateProtocol : IBowireProtocol
{
    public string Name => "Pirate Speak";

    public string Id => "pirate";

    public string IconSvg =>
        """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><circle cx="12" cy="12" r="10"/><path d="M8 14s1.5 2 4 2 4-2 4-2M9 9h.01M15 9h.01"/></svg>""";

    // Pirate Speak needs no host services — leave the IServiceProvider
    // ignored. The interface ships a default empty body but the
    // scaffold's tests call Initialize through a concrete-class
    // reference, so we declare it explicitly to keep that call valid
    // after the swap.
    public void Initialize(IServiceProvider? serviceProvider) { }

    public Task<List<BowireServiceInfo>> DiscoverAsync(
        string serverUrl, bool showInternalServices, CancellationToken ct = default)
    {
        var input = new BowireMessageInfo(
            Name: "TranslateRequest",
            FullName: "pirate.TranslateRequest",
            Fields: []);

        var output = new BowireMessageInfo(
            Name: "TranslateResponse",
            FullName: "pirate.TranslateResponse",
            Fields: []);

        var translate = new BowireMethodInfo(
            Name: "Translate",
            FullName: "BuccaneerService/Translate",
            ClientStreaming: false,
            ServerStreaming: false,
            InputType: input,
            OutputType: output,
            MethodType: "Unary")
        {
            Summary = "Convert plain English into pirate-speak.",
        };

        var service = new BowireServiceInfo(
            Name: "BuccaneerService",
            Package: Id,
            Methods: [translate])
        {
            Description = "The high seas, but JSON.",
        };

        return Task.FromResult<List<BowireServiceInfo>>([service]);
    }

    public Task<InvokeResult> InvokeAsync(
        string serverUrl, string service, string method,
        List<string> jsonMessages, bool showInternalServices,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var payload = jsonMessages.Count > 0 ? jsonMessages[0] : "{}";
        // System.Text.Json keeps this honest without a model class:
        // the wire payload is `{ "text": "..." }`, anything else falls
        // through to an empty string.
        var text = "";
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("text", out var t))
            {
                text = t.GetString() ?? "";
            }
        }
        catch (JsonException) { /* malformed payload → empty */ }

        var translated = Piratify(text);
        var response = JsonSerializer.Serialize(new { translated });

        return Task.FromResult(new InvokeResult(
            Response: response,
            DurationMs: 0,
            Status: "OK",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)));
    }

    public async IAsyncEnumerable<string> InvokeStreamAsync(
        string serverUrl, string service, string method,
        List<string> jsonMessages, bool showInternalServices,
        Dictionary<string, string>? metadata = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Pirate Speak is unary-only — return nothing for streaming
        // calls so the workbench's stream pane stays empty rather than
        // showing spurious frames.
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    public Task<IBowireChannel?> OpenChannelAsync(
        string serverUrl, string service, string method,
        bool showInternalServices, Dictionary<string, string>? metadata = null,
        CancellationToken ct = default)
        => Task.FromResult<IBowireChannel?>(null);

    // Tiny substitution table — proves the plugin's response really
    // comes from your code, not from the scaffold. Replace with an
    // actual transport call (HttpClient, gRPC channel, &c) when you
    // build a real protocol plugin.
    private static string Piratify(string text) => string.IsNullOrEmpty(text)
        ? "Ahoy, matey! 🏴‍☠️"
        : text
            .Replace(" the ", " th' ", StringComparison.OrdinalIgnoreCase)
            .Replace(" you ", " ye ", StringComparison.OrdinalIgnoreCase)
            .Replace(" my ", " me ", StringComparison.OrdinalIgnoreCase)
            .Replace(" your ", " yer ", StringComparison.OrdinalIgnoreCase)
            .Replace(" is ", " be ", StringComparison.OrdinalIgnoreCase)
            .Replace(" are ", " be ", StringComparison.OrdinalIgnoreCase)
            + " 🏴‍☠️";
}
