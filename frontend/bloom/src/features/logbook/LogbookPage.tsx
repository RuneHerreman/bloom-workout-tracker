import { useEffect, useState } from "react";
import "../../assets/css/logbook.css";
import { getLogs, deleteLog } from "./api.ts";
import type { LoggedWorkout } from "./api.ts";
import { searchExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import LogSideBar from "./components/LogSideBar.tsx";
import LogDetail from "./components/LogDetail.tsx";

const LogbookPage = () => {
    const [logs, setLogs] = useState<LoggedWorkout[]>([]);
    const [exercises, setExercises] = useState<Record<string, Exercise>>({});
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        Promise.all([getLogs(), searchExercises()]).then(([fetchedLogs, exs]) => {
            setLogs(fetchedLogs.sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime()));
            setExercises(Object.fromEntries(exs.map(e => [e.id, e])));
            setLoading(false);
        });
    }, []);

    const selectedLog = logs.find(l => l.id === selectedId) ?? null;

    const handleSave = (id: string, name: string, loggedAt: string, note: string | null) => {
        setLogs(prev =>
            prev
                .map(l => l.id === id ? { ...l, name, loggedAt, note } : l)
                .sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime())
        );
    };

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
            <HeaderComponent title="Logbook" subtitle="History" />
            <div className="panel-body">
                <LogSideBar
                    logs={logs}
                    selectedId={selectedId}
                    loading={loading}
                    onSelect={setSelectedId}
                />
                <div className="panel-detail">
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