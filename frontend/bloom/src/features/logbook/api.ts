import { fetchFromServer } from "../../assets/js/data/apiClient.ts";
import type { ExerciseType, WeightUnit, DistanceUnit } from "../../types.ts";
import type {
    LoggedSet,
    LoggedExercise,
    LoggedWorkout,
    ExercisePrResponse,
    ExerciseVolumeResponse,
} from "../../assets/js/data/apiTypes.ts";

export type { LoggedSet, LoggedExercise, LoggedWorkout, ExercisePrResponse, ExerciseVolumeResponse };

// ── Filter types ──────────────────────────────────────────────────────────────

export interface ExerciseFilters {
    name?: string;
    targetMuscleGroups?: string[];
    exerciseTypes?: ExerciseType[];
}

export interface VolumeFilters extends ExerciseFilters {
    fromYear?: number;
    fromMonth?: number;
    toYear?: number;
    toMonth?: number;
}

// ── Set factory functions ─────────────────────────────────────────────────────

export function createCardioSet(
    order: number,
    duration: string,
    distance: number,
    distanceUnit: DistanceUnit
): LoggedSet {
    return {
        type: "Cardio", order, duration, distance, distanceUnit,
        reps: null, weight: null, weightUnit: null, rir: null,
    };
}

export function createStrengthSet(
    order: number,
    reps: number,
    weight: number,
    weightUnit: WeightUnit,
    rir: number
): LoggedSet {
    return {
        type: "Strength", order, reps, weight, weightUnit, rir,
        duration: null, distance: null, distanceUnit: null,
    };
}

export function createPlyometricSet(
    order: number,
    reps: number,
    weight: number,
    weightUnit: WeightUnit,
    rir: number
): LoggedSet {
    return {
        type: "Plyometric", order, reps, weight, weightUnit, rir,
        duration: null, distance: null, distanceUnit: null,
    };
}

// ── GPX encoding ─────────────────────────────────────────────────────────────
// GPX XML is gzip-compressed then base64-encoded before sending. This reduces
// a typical 5 MB GPX file to ~50 KB on the wire and prevents EF Core OOM when
// it serializes the jsonb column.
// Detection: gzip magic bytes 0x1F 0x8B at the start of the decoded buffer.
// Fallback: plain base64 (previous format) and raw XML (oldest rows) both work.

async function gzipToBase64(data: Uint8Array): Promise<string> {
    const cs = new CompressionStream("gzip");
    const writer = cs.writable.getWriter();
    writer.write(data.buffer as ArrayBuffer);
    writer.close();
    const chunks: Uint8Array[] = [];
    const reader = cs.readable.getReader();
    for (;;) {
        const { done, value } = await reader.read();
        if (done) break;
        chunks.push(value);
    }
    const out = new Uint8Array(chunks.reduce((n, c) => n + c.length, 0));
    let off = 0;
    for (const c of chunks) { out.set(c, off); off += c.length; }
    let binary = "";
    out.forEach(b => binary += String.fromCharCode(b));
    return btoa(binary);
}

async function gunzipFromBase64(b64: string): Promise<Uint8Array> {
    const binary = atob(b64);
    const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));
    const ds = new DecompressionStream("gzip");
    const writer = ds.writable.getWriter();
    writer.write(bytes);
    writer.close();
    const chunks: Uint8Array[] = [];
    const reader = ds.readable.getReader();
    for (;;) {
        const { done, value } = await reader.read();
        if (done) break;
        chunks.push(value);
    }
    const out = new Uint8Array(chunks.reduce((n, c) => n + c.length, 0));
    let off = 0;
    for (const c of chunks) { out.set(c, off); off += c.length; }
    return out;
}

async function encodeGpx(xml: string): Promise<string> {
    return gzipToBase64(new TextEncoder().encode(xml));
}

async function decodeGpx(stored: string): Promise<string> {
    if (!stored || stored.trimStart().startsWith("<")) return stored; // legacy raw XML
    const binary = atob(stored);
    const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));
    if (bytes[0] === 0x1f && bytes[1] === 0x8b) {
        // gzip-compressed
        return new TextDecoder().decode(await gunzipFromBase64(stored));
    }
    // legacy plain base64
    return new TextDecoder().decode(bytes);
}

async function encodeExercises(exercises: LoggedExercise[]): Promise<LoggedExercise[]> {
    return Promise.all(exercises.map(async ex =>
        ex.gpxData ? { ...ex, gpxData: await encodeGpx(ex.gpxData) } : ex
    ));
}

async function decodeWorkout(log: LoggedWorkout): Promise<LoggedWorkout> {
    return {
        ...log,
        exercises: await Promise.all(log.exercises.map(async ex =>
            ex.gpxData ? { ...ex, gpxData: await decodeGpx(ex.gpxData) } : ex
        )),
    };
}

// ── API functions ─────────────────────────────────────────────────────────────

let _logsCache: Promise<LoggedWorkout[]> | null = null;

export function getLogs(): Promise<LoggedWorkout[]> {
    if (!_logsCache) {
        _logsCache = fetchFromServer<LoggedWorkout[]>("logs", "GET")
            .then(logs => Promise.all(logs.map(decodeWorkout)))
            .catch(e => { _logsCache = null; throw e; });
    }
    return _logsCache;
}

export async function getLog(logId: string): Promise<LoggedWorkout> {
    const log = await fetchFromServer<LoggedWorkout>(`logs/${logId}`, "GET");
    return decodeWorkout(log);
}

export async function createLog(
    name: string,
    exercises: LoggedExercise[],
    note?: string | null,
    loggedAt?: string
): Promise<string> {
    const response = await fetchFromServer<{ loggedWorkoutId: string }>("logs", "POST", {
        name,
        exercises: await encodeExercises(exercises),
        ...(note != null ? { note } : {}),
        ...(loggedAt ? { loggedAt } : {}),
    });
    _logsCache = null;
    _volumeCache = null;
    return response.loggedWorkoutId;
}

export async function updateLog(
    logId: string,
    name: string,
    loggedAt: string,
    exercises: LoggedExercise[],
    note?: string | null
): Promise<string> {
    const response = await fetchFromServer<{ loggedWorkoutId: string }>(`logs/${logId}`, "PUT", {
        name, loggedAt,
        exercises: await encodeExercises(exercises),
        ...(note != null ? { note } : {}),
    });
    _logsCache = null;
    _volumeCache = null;
    return response.loggedWorkoutId;
}

export async function deleteLog(logId: string): Promise<void> {
    await fetchFromServer<unknown>(`logs/${logId}`, "DELETE");
    _logsCache = null;
    _volumeCache = null;
}

export async function getPRs(filters?: ExerciseFilters): Promise<ExercisePrResponse[]> {
    return fetchFromServer<ExercisePrResponse[]>(`logs/pr${buildExerciseParams(filters)}`, "GET");
}

let _volumeCache: Promise<ExerciseVolumeResponse[]> | null = null;

export function getVolume(filters?: VolumeFilters): Promise<ExerciseVolumeResponse[]> {
    if (!filters) {
        if (!_volumeCache) {
            _volumeCache = fetchFromServer<ExerciseVolumeResponse[]>("logs/volume", "GET")
                .catch(e => { _volumeCache = null; throw e; });
        }
        return _volumeCache;
    }
    return fetchFromServer<ExerciseVolumeResponse[]>(`logs/volume${buildVolumeParams(filters)}`, "GET");
}

// ── Query param helpers ───────────────────────────────────────────────────────

function buildExerciseParams(filters?: ExerciseFilters): string {
    if (!filters) return "";
    const p = new URLSearchParams();
    if (filters.name) p.set("Name", filters.name);
    filters.targetMuscleGroups?.forEach(m => p.append("TargetMuscleGroups", m));
    filters.exerciseTypes?.forEach(t => p.append("ExerciseTypes", t));
    const s = p.toString();
    return s ? `?${s}` : "";
}

function buildVolumeParams(filters?: VolumeFilters): string {
    if (!filters) return "";
    const p = new URLSearchParams();
    if (filters.name) p.set("Name", filters.name);
    filters.targetMuscleGroups?.forEach(m => p.append("TargetMuscleGroups", m));
    filters.exerciseTypes?.forEach(t => p.append("ExerciseTypes", t));
    if (filters.fromYear !== undefined) p.set("FromYear", String(filters.fromYear));
    if (filters.fromMonth !== undefined) p.set("FromMonth", String(filters.fromMonth));
    if (filters.toYear !== undefined) p.set("ToYear", String(filters.toYear));
    if (filters.toMonth !== undefined) p.set("ToMonth", String(filters.toMonth));
    const s = p.toString();
    return s ? `?${s}` : "";
}
