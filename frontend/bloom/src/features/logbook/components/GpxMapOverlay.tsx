import { useEffect, useMemo, useRef, useState, useCallback } from "react";
import { MapContainer, TileLayer, Polyline, CircleMarker, Marker, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { Line, Bar } from "react-chartjs-2";
import {
    Chart as ChartJS, CategoryScale, LinearScale,
    PointElement, LineElement, Filler, Tooltip,
    BarController, BarElement,
} from "chart.js";
import type { ActiveElement, ChartEvent, TooltipItem } from "chart.js";
import type { GpxStats, GpxTrackPoint } from "../gpxUtils.ts";
import { formatDuration, formatPace, computeKmSplits } from "../gpxUtils.ts";
import { MapPin, TrendingUp, Clock, Ruler } from "lucide-react";
import L, { type LatLngTuple } from "leaflet";
import Overlay from "../../../components/general/OverlayComponent.tsx";

ChartJS.register(
    CategoryScale, LinearScale, PointElement, LineElement, Filler, Tooltip,
    BarController, BarElement,
);

interface GpxMapOverlayProps {
    points: GpxTrackPoint[];
    stats: GpxStats;
    onClose: () => void;
}

interface ChartConfig {
    key: string;
    title: string;
    unit: string;
    color: string;
    bg: string;
    points: GpxTrackPoint[];
    getValue: (p: GpxTrackPoint) => number;
    reverseY?: boolean;
    formatTick?: (v: number | string) => string;
    formatTooltip?: (v: string) => string;
}

// ── Static icon assets ────────────────────────────────────────────────────────

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

// ── Helpers ───────────────────────────────────────────────────────────────────

function downsample<T>(arr: T[], max: number): T[] {
    if (arr.length <= max) return arr;
    const step = arr.length / max;
    return Array.from({ length: max }, (_, i) => arr[Math.round(i * step)]);
}

function nearestIndex(pts: GpxTrackPoint[], distKm: number): number {
    return pts.reduce((best, p, i) =>
        Math.abs(p.distanceKm - distKm) < Math.abs(pts[best].distanceKm - distKm) ? i : best, 0
    );
}

function bearingDeg(lat1: number, lon1: number, lat2: number, lon2: number): number {
    const toRad = (d: number) => d * Math.PI / 180;
    const dLon = toRad(lon2 - lon1);
    const y = Math.sin(dLon) * Math.cos(toRad(lat2));
    const x = Math.cos(toRad(lat1)) * Math.sin(toRad(lat2))
             - Math.sin(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.cos(dLon);
    return (Math.atan2(y, x) * 180 / Math.PI + 360) % 360;
}

// ── Map sub-components ────────────────────────────────────────────────────────

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

// ── Main component ────────────────────────────────────────────────────────────

export default function GpxMapOverlay({ points, stats, onClose }: GpxMapOverlayProps) {
    const positions = useMemo<LatLngTuple[]>(() => points.map(p => [p.lat, p.lon]), [points]);
    const center: LatLngTuple = positions.length > 0 ? positions[Math.floor(positions.length / 2)] : [51.505, -0.09];

    const chartConfigs = useMemo<ChartConfig[]>(() => {
        const ds = (filter: (p: GpxTrackPoint) => boolean) => downsample(points.filter(filter), 300);
        const all: ChartConfig[] = [
            {
                key: "hr", title: "Heart Rate", unit: "bpm", color: "#c0392b", bg: "rgba(192,57,43,0.08)",
                points: ds(p => p.hr !== undefined),
                getValue: p => p.hr!,
            },
            {
                key: "pace", title: "Pace", unit: "min/km", color: "#528B8D", bg: "rgba(82,139,141,0.08)",
                points: ds(p => p.speedKph !== undefined && p.speedKph > 0.5),
                getValue: p => Math.round((60 / p.speedKph!) * 100) / 100,
                reverseY: true,
                formatTick: v => formatPace(typeof v === "number" ? v : parseFloat(v as string)),
                formatTooltip: v => ` ${formatPace(parseFloat(v))} /km`,
            },
            {
                key: "speed", title: "Speed", unit: "km/h", color: "#4A6E75", bg: "rgba(74,110,117,0.08)",
                points: ds(p => p.speedKph !== undefined),
                getValue: p => Math.round((p.speedKph ?? 0) * 10) / 10,
            },
            {
                key: "cad", title: "Cadence", unit: "rpm", color: "#8B6F47", bg: "rgba(139,111,71,0.08)",
                points: ds(p => p.cad !== undefined),
                getValue: p => p.cad!,
            },
            {
                key: "power", title: "Power", unit: "W", color: "#6B4E9B", bg: "rgba(107,78,155,0.08)",
                points: ds(p => p.power !== undefined),
                getValue: p => Math.round(p.power!),
            },
            {
                key: "grade", title: "Grade", unit: "%", color: "#E9762B", bg: "rgba(233,118,43,0.08)",
                points: ds(p => p.grade !== undefined),
                getValue: p => Math.round((p.grade ?? 0) * 10) / 10,
            },
            {
                key: "ele", title: "Elevation", unit: "m", color: "#2D8055", bg: "rgba(45,128,85,0.12)",
                points: ds(p => p.ele !== undefined),
                getValue: p => Math.round(p.ele!),
            },
            {
                key: "atemp", title: "Temperature", unit: "°C", color: "#3B7FC4", bg: "rgba(59,127,196,0.08)",
                points: ds(p => p.atemp !== undefined),
                getValue: p => Math.round(p.atemp! * 10) / 10,
            },
        ];
        return all.filter(c => c.points.length > 0);
    }, [points]);

    const splits = useMemo(() => computeKmSplits(points), [points]);
    const hasHr = splits.some(s => s.avgHr !== undefined);

    const splitsBarData = useMemo(() => {
        const minP = Math.min(...splits.map(s => s.pace));
        const maxP = Math.max(...splits.map(s => s.pace));
        const range = maxP - minP || 1;
        return {
            labels: splits.map(s => String(s.km)),
            datasets: [{
                data: splits.map(s => s.pace),
                backgroundColor: splits.map(s => {
                    const t = (s.pace - minP) / range;
                    const r = Math.round(45 + t * (233 - 45));
                    const g = Math.round(128 + t * (118 - 128));
                    const b = Math.round(85 + t * (43 - 85));
                    return `rgba(${r},${g},${b},0.75)`;
                }),
                borderRadius: 3,
                borderWidth: 0,
            }],
        };
    }, [splits]);

    const splitsBarOptions = useMemo(() => ({
        indexAxis: "y" as const,
        responsive: true,
        maintainAspectRatio: false,
        animation: false as const,
        plugins: {
            legend: { display: false },
            tooltip: {
                backgroundColor: "rgba(255,255,255,0.95)",
                titleColor: "#333", bodyColor: "#666",
                borderColor: "#e3e3e3", borderWidth: 1,
                padding: 10,
                callbacks: {
                    title: (items: { label: string }[]) => `km ${items[0]?.label}`,
                    label: (item: TooltipItem<"bar">) => {
                        const split = splits[item.dataIndex];
                        const parts = [` ${formatPace(item.raw as number)} /km  ${formatDuration(split.durationMs)}`];
                        if (split.avgHr) parts.push(` ♥ ${split.avgHr} bpm`);
                        return parts;
                    },
                },
            },
        },
        scales: {
            x: {
                border: { display: false },
                grid: { color: "#F0F0F0" },
                ticks: {
                    font: { size: 10 }, color: "#999", maxTicksLimit: 5,
                    callback: (v: number | string) =>
                        formatPace(typeof v === "number" ? v : parseFloat(v as string)),
                },
            },
            y: {
                border: { display: false },
                grid: { display: false },
                ticks: {
                    font: { size: 10 }, color: "#999",
                    callback: (_v: number | string, i: number) => `${i + 1} km`,
                },
            },
        },
    }), [splits]);

    const chartsRef = useRef<Record<string, ChartJS<"line">>>({});
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
        return points[nearestIndex(points, hoverDistKm)];
    }, [hoverDistKm, points]);

    useEffect(() => {
        for (const cfg of chartConfigs) {
            const chart = chartsRef.current[cfg.key];
            if (!chart || cfg.points.length === 0) continue;
            if (hoverDistKm === null) {
                chart.setActiveElements([]);
                chart.tooltip?.setActiveElements([], { x: 0, y: 0 });
            } else {
                const idx = nearestIndex(cfg.points, hoverDistKm);
                chart.setActiveElements([{ datasetIndex: 0, index: idx }]);
                chart.tooltip?.setActiveElements([{ datasetIndex: 0, index: idx }], { x: 0, y: 0 });
            }
            chart.update("none");
        }
    }, [hoverDistKm, chartConfigs]);

    const handleMapHover = useCallback((dist: number | null) => setHoverDistKm(dist), []);

    const makeChartOptions = (cfg: ChartConfig) => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false as const,
        interaction: { mode: "index" as const, intersect: false },
        onHover: (_: ChartEvent, els: ActiveElement[]) => {
            setHoverDistKm(els.length > 0 ? (cfg.points[els[0].index]?.distanceKm ?? null) : null);
        },
        plugins: {
            legend: { display: false },
            tooltip: {
                mode: "index" as const,
                intersect: false,
                backgroundColor: "rgba(255,255,255,0.95)",
                titleColor: "#333", bodyColor: "#666",
                borderColor: "#e3e3e3", borderWidth: 1,
                padding: 12, boxPadding: 6, usePointStyle: true,
                callbacks: {
                    title: (items: { label: string }[]) => `${items[0]?.label} km`,
                    label: (item: { formattedValue: string }) =>
                        cfg.formatTooltip ? cfg.formatTooltip(item.formattedValue) : ` ${item.formattedValue} ${cfg.unit}`,
                },
            },
        },
        scales: {
            x: { display: false },
            y: {
                reverse: cfg.reverseY ?? false,
                border: { display: false },
                grid: { color: "#F0F0F0" },
                ticks: {
                    font: { size: 10 }, color: "#999", maxTicksLimit: 4,
                    callback: cfg.formatTick ?? ((v: number | string) => `${v} ${cfg.unit}`),
                },
            },
        },
        elements: { point: { radius: 0, hoverRadius: 6, hoverBorderWidth: 2, hoverBorderColor: "#fff" }, line: { tension: 0.3 } },
    });

    const splitsBarHeight = `${Math.max(6, splits.length * 1.55)}rem`;

    return (
        <Overlay title="Route" subtitle="GPX" onClose={onClose} noPadding panelClassName="overlay-panel-wide">
            <div className="gpx-layout">

                {/* Left col: map + splits */}
                <div className="gpx-left-col">
                    <div className="gpx-map-cell">
                        <div className="gpx-map-inner">
                            <MapContainer center={center} zoom={13} style={{ width: "100%", height: "100%" }} zoomControl>
                                <TileLayer
                                    url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
                                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/">CARTO</a>'
                                />
                                <Polyline positions={positions} pathOptions={{ color: "#2D8055", weight: 3, opacity: 0.85 }} />
                                {positions.length > 0 && <Marker position={positions[0]} icon={START_ICON} />}
                                {positions.length > 1 && <Marker position={positions[positions.length - 1]} icon={END_ICON} />}
                                {arrowMarkers.map((a, i) => <Marker key={i} position={a.pos} icon={arrowIcon(a.deg)} />)}
                                {hoverMapPoint && (
                                    <CircleMarker center={[hoverMapPoint.lat, hoverMapPoint.lon]} radius={7}
                                        pathOptions={{ color: "#fff", fillColor: "#2D8055", fillOpacity: 1, weight: 2.5 }} />
                                )}
                                <FitBounds positions={positions} />
                                <MapHoverTracker points={points} onHover={handleMapHover} />
                            </MapContainer>
                        </div>
                    </div>
                    {splits.length > 0 && (
                        <div className="gpx-splits-cell">
                            <p className="gpx-section-title">Splits</p>
                            {hasHr && <p className="gpx-splits-meta">Hover for avg HR</p>}
                            <div className="gpx-splits-bar-area" style={{ height: splitsBarHeight }}>
                                <Bar data={splitsBarData} options={splitsBarOptions as never} />
                            </div>
                        </div>
                    )}
                </div>

                {/* Right col: charts spanning both rows */}
                <div className="gpx-charts-cell">
                    {chartConfigs.map(cfg => (
                        <div key={cfg.key} className="gpx-chart-wrap">
                            <p className="gpx-chart-title">{cfg.title}</p>
                            <div className="gpx-chart-area">
                                <Line
                                    ref={chart => { if (chart) chartsRef.current[cfg.key] = chart; else delete chartsRef.current[cfg.key]; }}
                                    data={{
                                        labels: cfg.points.map(p => p.distanceKm.toFixed(2)),
                                        datasets: [{
                                            data: cfg.points.map(cfg.getValue),
                                            borderColor: cfg.color, backgroundColor: cfg.bg,
                                            borderWidth: 1.5, fill: true,
                                            pointBackgroundColor: cfg.color, pointBorderColor: "#fff",
                                        }],
                                    }}
                                    options={makeChartOptions(cfg)}
                                />
                            </div>
                        </div>
                    ))}
                </div>

                {/* Stats footer — sticky at bottom */}
                <div className="gpx-map-stats">
                    <div className="gpx-map-stat"><Ruler size={14} /><span>{stats.distanceKm.toFixed(2)} km</span></div>
                    {stats.elevationGainM > 0 && <div className="gpx-map-stat"><TrendingUp size={14} /><span>+{Math.round(stats.elevationGainM)} m</span></div>}
                    {stats.durationMs > 0 && <div className="gpx-map-stat"><Clock size={14} /><span>{formatDuration(stats.durationMs)}</span></div>}
                    {stats.durationMs > 0 && stats.distanceKm > 0 && <div className="gpx-map-stat"><MapPin size={14} /><span>{formatPace(stats.durationMs / 60000 / stats.distanceKm)} /km</span></div>}
                </div>
            </div>
        </Overlay>
    );
}
