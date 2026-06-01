// Container-manifest store — an in-memory registry keyed by ISO 6346 code.
// Seeded with three sample containers so a fresh `dotnet run` already has
// something to enumerate. Thread-safe enough for the bootcamp scenario;
// a real harbour backend would back this with a database.

using System.Collections.Concurrent;

namespace HarborTour;

public sealed class ContainerManifestStore
{
    private readonly ConcurrentDictionary<string, ContainerManifest> _store = new();

    public ContainerManifestStore()
    {
        Upsert(new ContainerManifest(
            Id: "MAEU1234567",
            Carrier: "Maersk",
            OriginPort: "Hamburg",
            DestinationPort: "Rotterdam",
            Contents: "Electronics (HS 8517)",
            WeightTons: 18.4));
        Upsert(new ContainerManifest(
            Id: "HLCU7654321",
            Carrier: "Hapag-Lloyd",
            OriginPort: "Bremerhaven",
            DestinationPort: "Antwerp",
            Contents: "Automotive parts (HS 8708)",
            WeightTons: 24.7));
        Upsert(new ContainerManifest(
            Id: "MSCU1112223",
            Carrier: "MSC",
            OriginPort: "Hamburg",
            DestinationPort: "Felixstowe",
            Contents: "Pharmaceutical refrigerated cargo (HS 3004)",
            WeightTons: 12.1));
    }

    public IReadOnlyList<ContainerSummary> ListSummaries() =>
        _store.Values
            .OrderBy(m => m.Id, StringComparer.Ordinal)
            .Select(m => new ContainerSummary(m.Id, m.Carrier, m.DestinationPort))
            .ToList();

    public bool TryGet(string id, out ContainerManifest manifest) =>
        _store.TryGetValue(id, out manifest!);

    public void Upsert(ContainerManifest manifest) =>
        _store[manifest.Id] = manifest;
}

public sealed record ContainerManifest(
    string Id,
    string Carrier,
    string OriginPort,
    string DestinationPort,
    string Contents,
    double WeightTons);

public sealed record ContainerSummary(
    string Id,
    string Carrier,
    string DestinationPort);
