// Base map tiles shared by every map in the app.
//
// CARTO's basemaps now require an API key (unauthenticated tiles render an
// "API KEY REQUIRED" watermark), so maps use OpenStreetMap's standard tiles.
// OSM's tile policy requires visible attribution, so keep the attribution
// control enabled wherever these are used. A CSS filter mutes the colourful
// default style so routes stay the focus (see .map-tiles in logbook.css).

export const TILE_URL = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";

export const TILE_ATTRIBUTION =
    '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noreferrer">OpenStreetMap</a> contributors';

export function tileClassName(dark: boolean): string {
    return dark ? "map-tiles map-tiles-dark" : "map-tiles";
}
