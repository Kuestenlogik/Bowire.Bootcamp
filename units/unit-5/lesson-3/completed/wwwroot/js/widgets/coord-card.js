// Lesson 5.3 — COMPLETED. viewer.mount now reads the paired lat/lon from
// ctx.interpretations and renders a small card with an OpenStreetMap link —
// no external map library, so it stays offline-safe. The shipped
// Kuestenlogik.Bowire.Map package is the real MapLibre renderer for this
// same kind; this is the mechanism without the 800 KB dependency.
(function () {
    if (!window.BowireExtensions || typeof window.BowireExtensions.register !== 'function') {
        console.warn('[coord-card] BowireExtensions not present; skipping registration');
        return;
    }

    function renderPoint(kindMap) {
        var lat = kindMap && kindMap['coordinate.latitude'];
        var lon = kindMap && kindMap['coordinate.longitude'];
        if (lat == null || lon == null) { return null; }

        var row = document.createElement('div');
        row.className = 'bowire-coord-card-row';
        var osm = 'https://www.openstreetmap.org/?mlat=' + encodeURIComponent(lat)
            + '&mlon=' + encodeURIComponent(lon) + '#map=12/' + lat + '/' + lon;
        row.innerHTML = '📍 <strong>'
            + Number(lat).toFixed(4) + ', ' + Number(lon).toFixed(4)
            + '</strong> <a href="' + osm + '" target="_blank" rel="noopener">view</a>';
        return row;
    }

    window.BowireExtensions.register({
        id: 'bootcamp.coord-card',
        bowireApi: '1.x',
        kind: 'coordinate.wgs84',
        pairing: { required: ['coordinate.latitude', 'coordinate.longitude'], scope: 'same-parent' },
        viewer: {
            label: 'Coordinates',
            icon: 'map-pin',
            selectionMode: 'single',
            mount: function (container, ctx) {
                var card = document.createElement('div');
                card.className = 'bowire-coord-card';
                card.style.cssText = 'padding:8px 10px;border:1px solid var(--bowire-border,#334);'
                    + 'border-radius:6px;font:13px system-ui,sans-serif;';

                // Single-match mode → interpretations is one kindMap object.
                // Aggregate (multi) mode → an array of kindMaps, one per pairing.
                var kinds = ctx.interpretations || {};
                var maps = Array.isArray(kinds) ? kinds : [kinds];
                maps.forEach(function (k) {
                    var row = renderPoint(k);
                    if (row) { card.appendChild(row); }
                });

                if (!card.childNodes.length) {
                    card.textContent = 'No coordinate in this response.';
                }
                container.appendChild(card);

                return function () {
                    if (card.parentNode) { card.parentNode.removeChild(card); }
                };
            }
        }
    });
})();
