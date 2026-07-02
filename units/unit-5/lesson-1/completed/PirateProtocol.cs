// Lesson 5.1 — COMPLETED. The skeleton from ../start/ with the two TODOs
// filled in: DiscoverAsync advertises one unary "Translate" method, and
// InvokeAsync answers it by translating the request text into pirate speak.
// Pack it (`dotnet pack`) and `bowire plugin install` it, or add it as a
// PackageReference in an embedded host — both discover PirateProtocol.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Models;

namespace Bowire.Plugin.Pirate;

/// <summary>A toy protocol plugin that translates plain text into pirate speak.</summary>
public sealed class PirateProtocol : IBowireProtocol
{
    private const string ServiceName = "Pirate";
    private const string MethodName = "Translate";

    public string Name => "Pirate Speak";

    public string Id => "pirate";

    public string IconSvg =>
        """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 10h16"/><path d="M12 10v10"/><path d="M7 20h10"/><circle cx="12" cy="6" r="2"/></svg>""";

    // TODO 1 (done): advertise the "Pirate" service with a single unary method.
    public Task<List<BowireServiceInfo>> DiscoverAsync(
        string serverUrl, bool showInternalServices, CancellationToken ct = default)
    {
        var input = new BowireMessageInfo("TranslateRequest", $"{ServiceName}.TranslateRequest",
        [
            new BowireFieldInfo("text", 1, "string", "optional", false, false, null, null),
        ]);

        var output = new BowireMessageInfo("TranslateResponse", $"{ServiceName}.TranslateResponse",
        [
            new BowireFieldInfo("pirate", 1, "string", "optional", false, false, null, null),
        ]);

        var translate = new BowireMethodInfo(
            Name: MethodName,
            FullName: $"{ServiceName}/{MethodName}",
            ClientStreaming: false,
            ServerStreaming: false,
            InputType: input,
            OutputType: output,
            MethodType: "Unary");

        var service = new BowireServiceInfo(ServiceName, Id, [translate]);
        return Task.FromResult<List<BowireServiceInfo>>([service]);
    }

    // TODO 2 (done): read { "text": "..." }, translate, return { "pirate": "..." }.
    public Task<InvokeResult> InvokeAsync(
        string serverUrl, string service, string method,
        List<string> jsonMessages, bool showInternalServices,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var started = Stopwatch.GetTimestamp();

        var text = string.Empty;
        if (jsonMessages.Count > 0 && !string.IsNullOrWhiteSpace(jsonMessages[0]))
        {
            using var doc = JsonDocument.Parse(jsonMessages[0]);
            if (doc.RootElement.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
            {
                text = t.GetString() ?? string.Empty;
            }
        }

        var response = JsonSerializer.Serialize(new { pirate = ToPirate(text) });
        var elapsedMs = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;

        return Task.FromResult(new InvokeResult(
            Response: response,
            DurationMs: elapsedMs,
            Status: "OK",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)));
    }

    // Unary-only — nothing to stream, no channel to open.
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

    // A deliberately tiny word-swap translator — the point is the plugin
    // contract, not the linguistics.
    private static readonly (string From, string To)[] s_lexicon =
    [
        ("hello", "ahoy"), ("hi", "ahoy"), ("hey", "ahoy"),
        ("my", "me"), ("friend", "matey"), ("friends", "hearties"),
        ("yes", "aye"), ("is", "be"), ("are", "be"),
        ("you", "ye"), ("your", "yer"), ("the", "th'"),
        ("money", "doubloons"), ("treasure", "booty"), ("stop", "avast"),
    ];

    private static string ToPirate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "Arrr, speak up, matey!";
        }

        var words = text.Split(' ');
        for (var i = 0; i < words.Length; i++)
        {
            foreach (var (from, to) in s_lexicon)
            {
                if (string.Equals(words[i], from, StringComparison.OrdinalIgnoreCase))
                {
                    words[i] = to;
                    break;
                }
            }
        }

        return string.Join(' ', words) + ", arrr!";
    }
}
