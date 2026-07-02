// Lesson 5.3 — START. The C# descriptor is already complete: [BowireExtension]
// opts the type into the auto-discovery scan, and IBowireUiExtension declares
// which semantic kind it claims + which JS bundle to serve. The work of this
// lesson is in the JS bundle (wwwroot/js/widgets/coord-card.js) — the descriptor
// is boilerplate you rarely touch again.

using Kuestenlogik.Bowire.Semantics.Extensions;

namespace Bowire.Extension.CoordCard;

/// <summary>
/// Mounts a small coordinate card on the <c>coordinate.wgs84</c> semantic kind
/// — a dependency-free stand-in for the shipped <c>Kuestenlogik.Bowire.Map</c>
/// widget. All rendering lives in the embedded JS bundle.
/// </summary>
[BowireExtension]
public sealed class CoordCardExtension : IBowireUiExtension
{
    public string Id => "bootcamp.coord-card";

    public string BowireApiRange => "1.x";

    public IReadOnlyList<string> Kinds { get; } = ["coordinate.wgs84"];

    public ExtensionCapabilities Capabilities => ExtensionCapabilities.Viewer;

    public string BundleResourceName => "wwwroot/js/widgets/coord-card.js";

    public string? StylesResourceName => null;
}
