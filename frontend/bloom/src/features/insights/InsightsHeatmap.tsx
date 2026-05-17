import { useEffect, useRef, useState } from "react";
import { MapContainer, TileLayer, Polyline, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import type { LatLngTuple } from "leaflet";
import { List, X, LocateFixed } from "lucide-react";
import type { RoutePolyline } from "./cardioTransforms.ts";
import { useDarkModeContext } from "../../context/DarkModeContext.tsx";

const FALLBACK_CENTER: LatLngTuple = [51.505, -0.09];

interface InsightsHeatmapProps {
    routes: RoutePolyline[];
}

function SetView({ center }: { center: LatLngTuple }) {
    const map = useMap();
    useEffect(() => {
        map.setView(center, 11, { animate: false });
    }, [map, center]);
    return null;
}

function LocateButton({ center }: { center: LatLngTuple }) {
    const map = useMap();
    return (
        <button
            className="insights-heatmap-locate"
            title="Centre on my location"
            onClick={() => map.setView(center, 13, { animate: true })}
        >
            <LocateFixed size={14} />
        </button>
    );
}

function FitRoute({ route }: { route: RoutePolyline }) {
    const map = useMap();
    useEffect(() => {
        if (route.positions.length < 2) return;
        const lats = route.positions.map(p => p[0]);
        const lons = route.positions.map(p => p[1]);
        map.fitBounds(
            [[Math.min(...lats), Math.min(...lons)], [Math.max(...lats), Math.max(...lons)]],
            { padding: [36, 36], animate: true, duration: 0.5 }
        );
    }, [map, route]);
    return null;
}

function fmtDate(iso: string): string {
    return new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}

function fmtDist(km: number): string {
    return km >= 1 ? `${km.toFixed(1)} km` : `${Math.round(km * 1000)} m`;
}

export default function InsightsHeatmap({ routes }: InsightsHeatmapProps) {
    const { dark } = useDarkModeContext();
    const [geoCenter, setGeoCenter] = useState<LatLngTuple | null>(null);
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [pickerOpen, setPickerOpen] = useState(false);
    const pickerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!navigator.geolocation) return;
        navigator.geolocation.getCurrentPosition(
            pos => setGeoCenter([pos.coords.latitude, pos.coords.longitude]),
            () => {},
            { timeout: 5000 }
        );
    }, []);

    useEffect(() => {
        if (!pickerOpen) return;
        function onDown(e: MouseEvent) {
            if (pickerRef.current && !pickerRef.current.contains(e.target as Node))
                setPickerOpen(false);
        }
        document.addEventListener("mousedown", onDown);
        return () => document.removeEventListener("mousedown", onDown);
    }, [pickerOpen]);

    const selectedRoute = routes.find(r => r.id === selectedId) ?? null;

    function handleSelect(route: RoutePolyline) {
        setSelectedId(route.id);
        setPickerOpen(false);
    }

    function handleClear() {
        setSelectedId(null);
    }

    return (
        <div className="insights-heatmap">
            <MapContainer
                center={FALLBACK_CENTER}
                zoom={11}
                style={{ width: "100%", height: "100%" }}
                zoomControl={false}
                attributionControl={false}
            >
                <TileLayer
                    url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/">CARTO</a>'
                    className={dark ? "map-tiles-dark" : undefined}
                />

                {/* Heatmap layer — all routes at low opacity */}
                {routes.map(r => (
                    <Polyline
                        key={r.id}
                        positions={r.positions as LatLngTuple[]}
                        pathOptions={{
                            color: "#E9762B",
                            weight: selectedId && r.id !== selectedId ? 2 : 2.5,
                            opacity: selectedId
                                ? (r.id === selectedId ? 0 : 0.08)
                                : 0.18,
                        }}
                    />
                ))}

                {/* Selected route highlight on top */}
                {selectedRoute && (
                    <Polyline
                        positions={selectedRoute.positions as LatLngTuple[]}
                        pathOptions={{ color: "#E9762B", weight: 3.5, opacity: 0.9 }}
                    />
                )}

                {geoCenter && !selectedRoute && <SetView center={geoCenter} />}
                {selectedRoute && <FitRoute route={selectedRoute} />}
                {geoCenter && <LocateButton center={geoCenter} />}
            </MapContainer>

            {/* Route count badge */}
            <div className="insights-heatmap-badge">
                {routes.length} {routes.length === 1 ? "route" : "routes"}
            </div>

            {/* Run picker */}
            <div className="insights-heatmap-picker" ref={pickerRef}>
                <div className="insights-heatmap-picker-controls">
                    {selectedRoute && (
                        <button
                            className="insights-heatmap-clear"
                            onClick={handleClear}
                            title="Show all routes"
                        >
                            <X size={12} />
                        </button>
                    )}
                    <button
                        className={`insights-heatmap-picker-btn${pickerOpen ? " open" : ""}`}
                        onClick={() => setPickerOpen(o => !o)}
                        title="Jump to run"
                    >
                        <List size={13} />
                        {selectedRoute
                            ? <span>{selectedRoute.name}</span>
                            : <span>Jump to run</span>
                        }
                    </button>
                </div>

                {pickerOpen && (
                    <div className="insights-heatmap-picker-dropdown">
                        {routes.map(r => (
                            <button
                                key={r.id}
                                className={`insights-heatmap-picker-item${r.id === selectedId ? " selected" : ""}`}
                                onClick={() => handleSelect(r)}
                            >
                                <span className="insights-heatmap-picker-name">{r.name}</span>
                                <span className="insights-heatmap-picker-meta">
                                    {fmtDate(r.date)}
                                    {r.distanceKm > 0 && <> · {fmtDist(r.distanceKm)}</>}
                                </span>
                            </button>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}
