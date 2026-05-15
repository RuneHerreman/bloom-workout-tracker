import { useEffect, useMemo, useRef, useState, useCallback } from "react";
import { MapContainer, TileLayer, Polyline, CircleMarker, Marker, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { Line } from "react-chartjs-2";
import {
    Chart as ChartJS, CategoryScale, LinearScale,
    PointElement, LineElement, Filler, Tooltip,
} from "chart.js";
import type { ActiveElement, ChartEvent } from "chart.js";
import type { GpxStats, GpxTrackPoint } from "../gpxUtils.ts";
import { formatDuration } from "../gpxUtils.ts";
import { MapPin, TrendingUp, Clock, Ruler } from "lucide-react";
import L, { type LatLngTuple } from "leaflet";
import Overlay from "../../../components/general/OverlayComponent.tsx";

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Filler, Tooltip);

interface GpxMapOverlayProps {
    points: GpxTrackPoint[];
    stats: GpxStats;
    onClose: () => void;
}

function downsample<T>(arr: T[], max: number): T[] {
    if (arr.length <= max) return arr;
    const step = arr.length / max;
    return Array.from({ length: max }, (_, i) => arr[Math.round(i * step)]);
}

function bearingDeg(lat1: number, lon1: number, lat2: number, lon2: number): number {
    const toRad = (d: number) => d * Math.PI / 180;
    const dLon = toRad(lon2 - lon1);
    const y = Math.sin(dLon) * Math.cos(toRad(lat2));
    const x = Math.cos(toRad(lat1)) * Math.sin(toRad(lat2))
             - Math.sin(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.cos(dLon);
    return (Math.atan2(y, x) * 180 / Math.PI + 360) % 360;
}

const START_ICON = L.divIcon({
    className: "",
    iconSize: [22, 30],
    iconAnchor: [11, 30],
    html: `<svg width="22" height="30" viewBox="0 0 22 30" xmlns="http://www.w3.org/2000/svg">
             <path d="M11 0C4.9 0 0 4.9 0 11c0 7 11 19 11 19S22 18 22 11C22 4.9 17.1 0 11 0z"
                   fill="#2D8055" stroke="white" stroke-width="1.5"/>
             <polygon points="8,7.5 8,14.5 15.5,11" fill="white"/>
           </svg>`,
});

const END_ICON = L.divIcon({
    className: "",
    iconSize: [22, 30],
    iconAnchor: [11, 30],
    html: `<svg width="22" height="30" viewBox="0 0 22 30" xmlns="http://www.w3.org/2000/svg">
             <path d="M11 0C4.9 0 0 4.9 0 11c0 7 11 19 11 19S22 18 22 11C22 4.9 17.1 0 11 0z"
                   fill="#2c2c2c" stroke="white" stroke-width="1.5"/>
             <rect x="9.5" y="6" width="1.5" height="9" fill="white"/>
             <path d="M11 6.5 L17 8.5 L11 10.5 Z" fill="white"/>
           </svg>`,
});

function arrowIcon(deg: number) {
    return L.divIcon({
        className: "",
        iconSize: [20, 20],
        iconAnchor: [10, 10],
        html: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 20 20"
                    style="transform:rotate(${deg}deg);display:block;">
                 <circle cx="10" cy="10" r="9" fill="white" stroke="#2D8055" stroke-width="1.5"/>
                 <path d="M10 4 L15 15 L10 11.5 L5 15 Z" fill="#2D8055"/>
               </svg>`,
    });
}

function nearestIndex(pts: GpxTrackPoint[], distKm: number): number {
    return pts.reduce((best, p, i) =>
        Math.abs(p.distanceKm - distKm) < Math.abs(pts[best].distanceKm - distKm) ? i : best, 0
    );
}

function FitBounds({ positions }: { positions: LatLngTuple[] }) {
    const map = useMap();
    useEffect(() => {
        if (positions.length > 1) map.fitBounds(positions, { padding: [32, 32] });
    }, [map, positions]);
    return null;
}

function MapHoverTracker({ points, onHover }: { points: GpxTrackPoint[], onHover: (dist: number | null) => void }) {
    const map = useMap();
    useEffect(() => {
        const onMouseMove = (e: L.LeafletMouseEvent) => {
            let minDist = Infinity, minDistKm = 0;
            for (const p of points) {
                const d = (p.lat - e.latlng.lat) ** 2 + (p.lon - e.latlng.lng) ** 2;
                if (d < minDist) { minDist = d; minDistKm = p.distanceKm; }
            }
            onHover(minDistKm);
        };
        const onMouseOut = () => onHover(null);
        map.on("mousemove", onMouseMove);
        map.on("mouseout", onMouseOut);
        return () => { map.off("mousemove", onMouseMove); map.off("mouseout", onMouseOut); };
    }, [map, points, onHover]);
    return null;
}

export default function GpxMapOverlay({ points, stats, onClose }: GpxMapOverlayProps) {
    const positions = useMemo<LatLngTuple[]>(() => points.map(p => [p.lat, p.lon]), [points]);
    const center: LatLngTuple = positions.length > 0
        ? positions[Math.floor(positions.length / 2)]
        : [51.505, -0.09];

    const elePoints   = useMemo(() => downsample(points.filter(p => p.ele      !== undefined), 300), [points]);
    const hrPoints    = useMemo(() => downsample(points.filter(p => p.hr       !== undefined), 300), [points]);
    const speedPoints = useMemo(() => downsample(points.filter(p => p.speedKph !== undefined), 300), [points]);
    const cadPoints   = useMemo(() => downsample(points.filter(p => p.cad      !== undefined), 300), [points]);

    const eleChartRef   = useRef<ChartJS<"line">>(null);
    const hrChartRef    = useRef<ChartJS<"line">>(null);
    const speedChartRef = useRef<ChartJS<"line">>(null);
    const cadChartRef   = useRef<ChartJS<"line">>(null);
    const [hoverDistKm, setHoverDistKm] = useState<number | null>(null);

    const arrowMarkers = useMemo(() => {
        if (positions.length < 4) return [];
        return [0.2, 0.4, 0.6, 0.8].map(frac => {
            const i = Math.floor(frac * (positions.length - 2));
            const [lat1, lon1] = positions[i];
            const [lat2, lon2] = positions[i + 1];
            return { pos: positions[i] as LatLngTuple, deg: bearingDeg(lat1, lon1, lat2, lon2) };
        });
    }, [positions]);

    const hoverMapPoint = useMemo(() => {
        if (hoverDistKm === null || points.length === 0) return null;
        const idx = nearestIndex(points, hoverDistKm);
        return points[idx];
    }, [hoverDistKm, points]);

    // Sync chart active elements when hover comes from the map
    useEffect(() => {
        const sync = (chart: ChartJS<"line"> | null, pts: GpxTrackPoint[]) => {
            if (!chart || pts.length === 0) return;
            if (hoverDistKm === null) {
                chart.setActiveElements([]);
                chart.tooltip?.setActiveElements([], { x: 0, y: 0 });
            } else {
                const idx = nearestIndex(pts, hoverDistKm);
                chart.setActiveElements([{ datasetIndex: 0, index: idx }]);
                chart.tooltip?.setActiveElements([{ datasetIndex: 0, index: idx }], { x: 0, y: 0 });
            }
            chart.update("none");
        };
        sync(eleChartRef.current,   elePoints);
        sync(hrChartRef.current,    hrPoints);
        sync(speedChartRef.current, speedPoints);
        sync(cadChartRef.current,   cadPoints);
    }, [hoverDistKm, elePoints, hrPoints, speedPoints, cadPoints]);

    const handleEleHover   = useCallback((_: ChartEvent, els: ActiveElement[]) => {
        setHoverDistKm(els.length > 0 ? (elePoints[els[0].index]?.distanceKm   ?? null) : null);
    }, [elePoints]);
    const handleHrHover    = useCallback((_: ChartEvent, els: ActiveElement[]) => {
        setHoverDistKm(els.length > 0 ? (hrPoints[els[0].index]?.distanceKm    ?? null) : null);
    }, [hrPoints]);
    const handleSpeedHover = useCallback((_: ChartEvent, els: ActiveElement[]) => {
        setHoverDistKm(els.length > 0 ? (speedPoints[els[0].index]?.distanceKm ?? null) : null);
    }, [speedPoints]);
    const handleCadHover   = useCallback((_: ChartEvent, els: ActiveElement[]) => {
        setHoverDistKm(els.length > 0 ? (cadPoints[els[0].index]?.distanceKm   ?? null) : null);
    }, [cadPoints]);

    const handleMapHover = useCallback((dist: number | null) => {
        setHoverDistKm(dist);
    }, []);

    const chartOptions = (unit: string, onHover: (e: ChartEvent, els: ActiveElement[]) => void) => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false as const,
        interaction: { mode: "index" as const, intersect: false },
        onHover,
        plugins: {
            legend: { display: false },
            tooltip: {
                mode: "index" as const,
                intersect: false,
                backgroundColor: "rgba(255,255,255,0.95)",
                titleColor: "#333",
                bodyColor: "#666",
                borderColor: "#e3e3e3",
                borderWidth: 1,
                padding: 12,
                boxPadding: 6,
                usePointStyle: true,
                callbacks: {
                    title: (items: { label: string }[]) => `${items[0]?.label} km`,
                    label: (item: { formattedValue: string }) => ` ${item.formattedValue} ${unit}`,
                },
            },
        },
        scales: {
            x: { display: false },
            y: {
                border: { display: false },
                grid: { color: "#F0F0F0" },
                ticks: { font: { size: 10 }, color: "#999", maxTicksLimit: 4,
                    callback: (v: number | string) => `${v} ${unit}` },
            },
        },
        elements: { point: { radius: 0, hoverRadius: 6, hoverBorderWidth: 2, hoverBorderColor: "#fff" }, line: { tension: 0.3 } },
    });

    const elevationData = {
        labels: elePoints.map(p => p.distanceKm.toFixed(2)),
        datasets: [{
            data: elePoints.map(p => p.ele!),
            borderColor: "#2D8055", backgroundColor: "rgba(45,128,85,0.12)",
            borderWidth: 1.5, fill: true,
            pointBackgroundColor: "#2D8055", pointBorderColor: "#fff",
        }],
    };

    const hrData = {
        labels: hrPoints.map(p => p.distanceKm.toFixed(2)),
        datasets: [{
            data: hrPoints.map(p => p.hr!),
            borderColor: "#c0392b", backgroundColor: "rgba(192,57,43,0.08)",
            borderWidth: 1.5, fill: true,
            pointBackgroundColor: "#c0392b", pointBorderColor: "#fff",
        }],
    };

    const speedData = {
        labels: speedPoints.map(p => p.distanceKm.toFixed(2)),
        datasets: [{
            data: speedPoints.map(p => Math.round((p.speedKph ?? 0) * 10) / 10),
            borderColor: "#528B8D", backgroundColor: "rgba(82,139,141,0.08)",
            borderWidth: 1.5, fill: true,
            pointBackgroundColor: "#528B8D", pointBorderColor: "#fff",
        }],
    };

    const cadData = {
        labels: cadPoints.map(p => p.distanceKm.toFixed(2)),
        datasets: [{
            data: cadPoints.map(p => p.cad!),
            borderColor: "#8B6F47", backgroundColor: "rgba(139,111,71,0.08)",
            borderWidth: 1.5, fill: true,
            pointBackgroundColor: "#8B6F47", pointBorderColor: "#fff",
        }],
    };

    return (
        <Overlay title="Route" subtitle="GPX" onClose={onClose} noPadding panelClassName="overlay-panel-wide">
            <div className="gpx-map-container">
                <div className="gpx-map-inner">
                    <MapContainer center={center} zoom={13} style={{ width: "100%", height: "100%" }} zoomControl={true}>
                        <TileLayer
                            url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
                            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/">CARTO</a>'
                        />
                        <Polyline positions={positions} pathOptions={{ color: "#2D8055", weight: 3, opacity: 0.85 }} />
                        {positions.length > 0 && (
                            <Marker position={positions[0]} icon={START_ICON} />
                        )}
                        {positions.length > 1 && (
                            <Marker position={positions[positions.length - 1]} icon={END_ICON} />
                        )}
                        {arrowMarkers.map((a, i) => (
                            <Marker key={i} position={a.pos} icon={arrowIcon(a.deg)} />
                        ))}
                        {hoverMapPoint && (
                            <CircleMarker
                                center={[hoverMapPoint.lat, hoverMapPoint.lon]}
                                radius={7}
                                pathOptions={{ color: "#fff", fillColor: "#2D8055", fillOpacity: 1, weight: 2.5 }}
                            />
                        )}
                        <FitBounds positions={positions} />
                        <MapHoverTracker points={points} onHover={handleMapHover} />
                    </MapContainer>
                </div>
            </div>
            {(elePoints.length > 0 || hrPoints.length > 0 || speedPoints.length > 0 || cadPoints.length > 0) && (
                <div className="gpx-charts">
                    {hrPoints.length > 0 && (
                        <div className="gpx-chart-wrap">
                            <p className="gpx-chart-title">Heart Rate</p>
                            <div className="gpx-chart-area">
                                <Line ref={hrChartRef} data={hrData} options={chartOptions("bpm", handleHrHover)} />
                            </div>
                        </div>
                    )}
                    {speedPoints.length > 0 && (
                        <div className="gpx-chart-wrap">
                            <p className="gpx-chart-title">Speed</p>
                            <div className="gpx-chart-area">
                                <Line ref={speedChartRef} data={speedData} options={chartOptions("km/h", handleSpeedHover)} />
                            </div>
                        </div>
                    )}
                    {cadPoints.length > 0 && (
                        <div className="gpx-chart-wrap">
                            <p className="gpx-chart-title">Cadence</p>
                            <div className="gpx-chart-area">
                                <Line ref={cadChartRef} data={cadData} options={chartOptions("rpm", handleCadHover)} />
                            </div>
                        </div>
                    )}
                    {elePoints.length > 0 && (
                        <div className="gpx-chart-wrap">
                            <p className="gpx-chart-title">Elevation</p>
                            <div className="gpx-chart-area">
                                <Line ref={eleChartRef} data={elevationData} options={chartOptions("m", handleEleHover)} />
                            </div>
                        </div>
                    )}
                </div>
            )}
            <div className="gpx-map-stats">
                <div className="gpx-map-stat"><Ruler size={14} /><span>{stats.distanceKm.toFixed(2)} km</span></div>
                {stats.elevationGainM > 0 && <div className="gpx-map-stat"><TrendingUp size={14} /><span>+{Math.round(stats.elevationGainM)} m</span></div>}
                {stats.durationMs > 0 && <div className="gpx-map-stat"><Clock size={14} /><span>{formatDuration(stats.durationMs)}</span></div>}
                {stats.durationMs > 0 && stats.distanceKm > 0 && <div className="gpx-map-stat"><MapPin size={14} /><span>{formatDuration(stats.durationMs / stats.distanceKm)} /km</span></div>}
            </div>
        </Overlay>
    );
}
