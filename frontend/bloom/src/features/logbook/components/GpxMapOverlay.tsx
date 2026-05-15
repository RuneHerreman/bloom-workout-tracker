import { useEffect } from "react";
import { MapContainer, TileLayer, Polyline, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import type { GpxStats, GpxTrackPoint } from "../gpxUtils.ts";
import { formatDuration } from "../gpxUtils.ts";
import { MapPin, TrendingUp, Clock, Ruler, X } from "lucide-react";
import type { LatLngTuple } from "leaflet";

interface GpxMapOverlayProps {
    points: GpxTrackPoint[];
    stats: GpxStats;
    onClose: () => void;
}

function FitBounds({ positions }: { positions: LatLngTuple[] }) {
    const map = useMap();
    useEffect(() => {
        if (positions.length > 1) map.fitBounds(positions, { padding: [32, 32] });
    }, [map, positions]);
    return null;
}

export default function GpxMapOverlay({ points, stats, onClose }: GpxMapOverlayProps) {
    const positions: LatLngTuple[] = points.map(p => [p.lat, p.lon]);
    const center: LatLngTuple = positions.length > 0
        ? positions[Math.floor(positions.length / 2)]
        : [51.505, -0.09];

    return (
        <div className="overlay-backdrop" onClick={onClose}>
            <div className="gpx-map-panel" onClick={e => e.stopPropagation()}>
                <div className="gpx-map-header">
                    <span className="gpx-map-title">Route</span>
                    <button className="overlay-close" onClick={onClose}><X size={16} /></button>
                </div>
                <div className="gpx-map-container">
                    <MapContainer center={center} zoom={13} style={{ width: "100%", height: "24rem" }} zoomControl={true}>
                        <TileLayer
                            url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
                            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/">CARTO</a>'
                        />
                        <Polyline positions={positions} pathOptions={{ color: "#2D8055", weight: 3, opacity: 0.85 }} />
                        <FitBounds positions={positions} />
                    </MapContainer>
                </div>
                <div className="gpx-map-stats">
                    <div className="gpx-map-stat">
                        <Ruler size={14} />
                        <span>{stats.distanceKm.toFixed(2)} km</span>
                    </div>
                    {stats.elevationGainM > 0 && (
                        <div className="gpx-map-stat">
                            <TrendingUp size={14} />
                            <span>+{Math.round(stats.elevationGainM)} m</span>
                        </div>
                    )}
                    {stats.durationMs > 0 && (
                        <div className="gpx-map-stat">
                            <Clock size={14} />
                            <span>{formatDuration(stats.durationMs)}</span>
                        </div>
                    )}
                    {stats.durationMs > 0 && stats.distanceKm > 0 && (
                        <div className="gpx-map-stat">
                            <MapPin size={14} />
                            <span>{formatDuration(stats.durationMs / stats.distanceKm)} /km</span>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}