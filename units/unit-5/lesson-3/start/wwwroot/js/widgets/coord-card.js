// Lesson 5.3 — START. Registers a viewer for the `coordinate.wgs84` kind, but
// viewer.mount only renders a placeholder. Your task: read the paired lat/lon
// from `ctx.interpretations` and render them. Diff against
// ../../../../completed/wwwroot/js/widgets/coord-card.js.
(function () {
    if (!window.BowireExtensions || typeof window.BowireExtensions.register !== 'function') {
        console.warn('[coord-card] BowireExtensions not present; skipping registration');
        return;
    }

    window.BowireExtensions.register({
        id: 'bootcamp.coord-card',
        bowireApi: '1.x',
        kind: 'coordinate.wgs84',
        // Pairing: a WGS84 point is a latitude + longitude field under the
        // same parent object (same shape the built-in map widget uses).
        pairing: { required: ['coordinate.latitude', 'coordinate.longitude'], scope: 'same-parent' },
        viewer: {
            label: 'Coordinates',
            icon: 'map-pin',
            selectionMode: 'single',
            mount: function (container, ctx) {
                // TODO: read ctx.interpretations['coordinate.latitude'] and
                // ctx.interpretations['coordinate.longitude'] and render a card.
                var el = document.createElement('div');
                el.textContent = 'coord-card: implement viewer.mount';
                container.appendChild(el);

                // A mount MUST return an unmount function so the framework can
                // tear the widget down when the response pane changes.
                return function () {
                    if (el.parentNode) { el.parentNode.removeChild(el); }
                };
            }
        }
    });
})();
