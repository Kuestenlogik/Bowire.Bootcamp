// Drop this row into the Seed array in
// samples/Kuestenlogik.Bowire.Sample.TacticalApi/Program.cs (look for the
// `private static readonly (string Uuid, string Name, double Lat, double Lon)[] Seed`
// declaration around line 95).
//
// Pick a coordinate in the DACH region so the new pin lands inside the
// existing map viewport. Basel (47.5596, 7.5886) is a good choice — it
// sits between the existing Bravo-2 (Munich) and Golf-7 (Zürich) seeds.
//
// After restart + re-invoking Situation.GetSituationObjects the response
// has nine objects and the map shows nine pins. No proto / schema change
// is needed — Wgs84CoordinateDetector picks the new entry up via the
// existing latitudeCoordinate / longitudeCoordinate field names.

("c5b3b5b6-1a2d-4e9b-8c0a-1f7a2d9c1a09", "India-9 — Field Hospital", 47.5596, 7.5886),
