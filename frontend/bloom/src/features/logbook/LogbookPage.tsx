import { useEffect, useState } from "react";
import "../../assets/css/logbook.css";
import { getLogs, createLog, deleteLog } from "./api.ts";
import type { LoggedWorkout } from "./api.ts";
import type { LoggedExercise, LoggedSet } from "../../assets/js/data/apiTypes.ts";
import { searchExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import type { WorkoutTemplate, PlannedSet } from "../templates/api.ts";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import Button from "../../components/general/ButtonComponent.tsx";
import LogSideBar from "./components/LogSideBar.tsx";
import LogDetail from "./components/LogDetail.tsx";
import StartSessionOverlay from "./components/StartSessionOverlay.tsx";
import { Plus, ChevronLeft } from "lucide-react";

function plannedToLogged(s: PlannedSet, order: number): LoggedSet {
    if (s.type === "Cardio") {
        return { type: "Cardio", order, reps: null, weight: null, weightUnit: null, rir: null,
            duration: s.duration, distance: s.distance, distanceUnit: s.distanceUnit };
    }
    return { type: s.type, order, reps: s.reps, weight: 60, weightUnit: "kg", rir: 2,
        duration: null, distance: null, distanceUnit: null };
}

const LogbookPage = () => {
    const [logs, setLogs] = useState<LoggedWorkout[]>([]);
    const [exercises, setExercises] = useState<Record<string, Exercise>>({});
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [startOpen, setStartOpen] = useState(false);

    useEffect(() => {
        Promise.all([getLogs(), searchExercises()]).then(([fetchedLogs, exs]) => {
            setLogs(fetchedLogs.sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime()));
            setExercises(Object.fromEntries(exs.map(e => [e.id, e])));
            setLoading(false);
        });
    }, []);

    const selectedLog = logs.find(l => l.id === selectedId) ?? null;

    const handleSave = (id: string, name: string, loggedAt: string, note: string | null, exercises: LoggedExercise[]) => {
        setLogs(prev =>
            prev
                .map(l => l.id === id ? { ...l, name, loggedAt, note, exercises } : l)
                .sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime())
        );
    };

    async function handleStartFromTemplate(template: WorkoutTemplate) {
        setStartOpen(false);
        const exercises: LoggedExercise[] = template.exercises
            .sort((a, b) => a.order - b.order)
            .map((ex, i) => ({
                exerciseId: ex.exerciseId,
                order: i + 1,
                gpxData: null,
                sets: ex.sets.sort((a, b) => a.order - b.order).map((s, j) => plannedToLogged(s, j + 1)),
            }));
        const id = await createLog(template.name, exercises);
        const fresh = await getLogs();
        setLogs(fresh.sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime()));
        setSelectedId(id);
    }

    async function handleStartBlank() {
        setStartOpen(false);
        const id = await createLog("Untitled Log", []);
        const fresh = await getLogs();
        setLogs(fresh.sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime()));
        setSelectedId(id);
    }

    const handleDelete = async (id: string) => {
        setLogs(prev => prev.filter(l => l.id !== id));
        if (selectedId === id) setSelectedId(null);
        try {
            await deleteLog(id);
        } catch {
            const fetched = await getLogs();
            setLogs(fetched.sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime()));
        }
    };

    return (
        <div className="panel-page">
            {startOpen && (
                <StartSessionOverlay
                    onFromTemplate={handleStartFromTemplate}
                    onBlank={handleStartBlank}
                    onClose={() => setStartOpen(false)}
                />
            )}
            <HeaderComponent
                title="Logbook"
                subtitle="History"
                action={<Button text="Log Workout" style="green" icon={<Plus size={14} />} onClick={() => setStartOpen(true)} />}
            />
            <div className={`panel-body${selectedId ? " has-selection" : ""}`}>
                <LogSideBar
                    logs={logs}
                    selectedId={selectedId}
                    loading={loading}
                    onSelect={setSelectedId}
                />
                <div className="panel-detail">
                    <button className="panel-back-btn" onClick={() => setSelectedId(null)}>
                        <ChevronLeft size={16} /> Logbook
                    </button>
                    {selectedLog ? (
                        <LogDetail
                            key={selectedLog.id}
                            log={selectedLog}
                            exercises={exercises}
                            onSave={handleSave}
                            onDelete={handleDelete}
                        />
                    ) : (
                        <div className="panel-empty">
                            <p>Select a log to see details</p>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default LogbookPage;