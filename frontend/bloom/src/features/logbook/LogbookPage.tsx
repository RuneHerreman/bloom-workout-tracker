import { useEffect, useMemo, useState, useCallback, useRef } from "react";
import { useLocation, useBlocker, useNavigate } from "react-router-dom";
import "../../assets/css/logbook.css";
import { getLogs, createLog, deleteLog, peekLogs } from "./api.ts";
import type { LoggedWorkout } from "./api.ts";
import type { LoggedExercise, LoggedSet } from "../../assets/js/data/apiTypes.ts";
import { searchExercises, peekExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import type { WorkoutTemplate, PlannedSet, TemplateExercise } from "../templates/api.ts";
import { createTemplate, getTemplates } from "../templates/api.ts";
import { getMe, peekMe } from "../auth/api.ts";
import ExportOverlay from "../export/ExportOverlay.tsx";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import Button from "../../components/general/ButtonComponent.tsx";
import LogSideBar from "./components/LogSideBar.tsx";
import LogDetail from "./components/LogDetail.tsx";
import StartSessionOverlay from "./components/StartSessionOverlay.tsx";
import DetailSkeleton from "../../components/general/DetailSkeleton.tsx";
import UnsavedChangesDialog from "../../components/general/UnsavedChangesDialog.tsx";
import { useToast } from "../../context/ToastContext.tsx";
import { Plus, ChevronLeft, Download, Upload } from "lucide-react";
import { useShortcut } from "../../hooks/useShortcut.ts";
import { buildPreviousSetsLookup, hintedSet, type PreviousSetsLookup } from "./logbookUtils.ts";
import { DRAFT_ID, createDraft, isDraftId, loadDraft, saveDraft } from "./drafts.ts";

const newestFirst = (a: LoggedWorkout, b: LoggedWorkout) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime();

/**
 * Template sets become empty logged sets whose placeholders show the plan, else
 * what you did last time (same set position, else the first set). Exercises
 * you've never logged and have no plan for start fully empty.
 */
function plannedToLogged(s: PlannedSet, order: number, previous: LoggedSet[] | undefined): LoggedSet {
    const ref = previous?.[order - 1] ?? previous?.[0];
    const set = hintedSet(s.type, order, ref);
    const hint = set.hint ?? {};
    return s.type === "Cardio"
        ? { ...set, distanceUnit: s.distanceUnit ?? set.distanceUnit,
            hint: { ...hint, duration: s.duration ?? hint.duration ?? null, distance: s.distance ?? hint.distance ?? null } }
        : { ...set, hint: { ...hint, reps: s.reps ?? hint.reps ?? null } };
}

function templateToExercises(template: WorkoutTemplate, previousSets: PreviousSetsLookup): LoggedExercise[] {
    return [...template.exercises]
        .sort((a, b) => a.order - b.order)
        .map((ex, i) => {
            const previous = previousSets(ex.exerciseId);
            return {
                exerciseId: ex.exerciseId,
                order: i + 1,
                gpxData: null,
                note: ex.note ?? null,
                gear: ex.gear ?? [],
                sets: [...ex.sets].sort((a, b) => a.order - b.order).map((s, j) => plannedToLogged(s, j + 1, previous)),
            };
        });
}

type NavState = { selectedLogId?: string; openMap?: boolean; openStart?: boolean; startTemplateId?: string } | null;

const LogbookPage = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { showToast } = useToast();
    // Render straight from the in-memory cache when it's warm (prefetched or visited before).
    const [logs, setLogs] = useState<LoggedWorkout[]>(() => [...(peekLogs() ?? [])].sort(newestFirst));
    const [draft, setDraftState] = useState<LoggedWorkout | null>(loadDraft);
    const [exercises, setExercises] = useState<Record<string, Exercise>>(
        () => Object.fromEntries((peekExercises() ?? []).map(e => [e.id, e]))
    );
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [dirtyInfo, setDirtyInfo] = useState<{ save: () => Promise<void>; name: string; date: string } | null>(null);
    const [pendingAction, setPendingAction] = useState<{ type: 'select'; id: string } | { type: 'back' } | null>(null);
    const [dialogSaving, setDialogSaving] = useState(false);
    const selectedLogRef = useRef<LoggedWorkout | null>(null);
    const handledTemplateStart = useRef<string | null>(null);

    const blocker = useBlocker(() => dirtyInfo !== null);
    const navState = location.state as NavState;

    useEffect(() => {
        setSelectedId(navState?.selectedLogId ?? null);
        if (navState?.openStart) setStartOpen(true);
    }, [location.key]); // eslint-disable-line react-hooks/exhaustive-deps

    const openMap = !!navState?.openMap;
    const [loading, setLoading] = useState(() => peekLogs() === undefined);
    const [startOpen, setStartOpen] = useState(() => !!navState?.openStart);
    const [newlyCreatedId, setNewlyCreatedId] = useState<string | null>(null);
    const [userGear, setUserGear] = useState<string[]>(() => peekMe()?.gear ?? []);
    const [exportOpen, setExportOpen] = useState(false);

    useEffect(() => {
        getLogs().then(fetchedLogs => {
            setLogs([...fetchedLogs].sort(newestFirst));
            setLoading(false);
        }).catch(() => setLoading(false));
        searchExercises().then(exs => {
            setExercises(Object.fromEntries(exs.map(e => [e.id, e])));
        });
        getMe().then(u => setUserGear(u.gear ?? [])).catch(() => null);
    }, []);

    const setDraft = useCallback((next: LoggedWorkout | null) => {
        setDraftState(next);
        saveDraft(next);
    }, []);

    const handleExerciseCreated = useCallback((exercise: Exercise) => {
        setExercises(prev => ({ ...prev, [exercise.id]: exercise }));
    }, []);

    const selectedLog = useMemo(
        () => (isDraftId(selectedId) ? draft : logs.find(l => l.id === selectedId) ?? null),
        [logs, selectedId, draft]
    );
    selectedLogRef.current = selectedLog;

    const previousSets = useMemo(
        () => buildPreviousSetsLookup(logs, isDraftId(selectedId) ? undefined : selectedId ?? undefined),
        [logs, selectedId]
    );

    const handleSave = (id: string, name: string, loggedAt: string, note: string | null, exercises: LoggedExercise[]) => {
        setLogs(prev => prev.map(l => l.id === id ? { ...l, name, loggedAt, note, exercises } : l).sort(newestFirst));
    };

    // ── Drafts ────────────────────────────────────────────────────────────────

    /** Replaces any existing draft, offering to bring the old one back. */
    const startDraft = useCallback((next: LoggedWorkout) => {
        const previous = draft;
        setDraft(next);
        setSelectedId(DRAFT_ID);
        setNewlyCreatedId(`${DRAFT_ID}-${Date.now()}`);
        if (previous && (previous.exercises.length > 0 || previous.name.trim())) {
            showToast({
                message: `Previous draft “${previous.name || "Untitled"}” discarded`,
                actionLabel: "Undo",
                onAction: () => { setDraft(previous); setSelectedId(DRAFT_ID); },
            });
        }
    }, [draft, setDraft, showToast]);

    const handleStartFromTemplate = useCallback((template: WorkoutTemplate) => {
        setStartOpen(false);
        startDraft(createDraft(template.name, templateToExercises(template, previousSets)));
    }, [previousSets, startDraft]);

    function handleStartBlank() {
        setStartOpen(false);
        startDraft(createDraft("", []));
    }

    function handleResumeDraft() {
        setStartOpen(false);
        setSelectedId(DRAFT_ID);
    }

    // Draft edits are mirrored to state (sidebar) and local storage (survives refresh).
    const handleDraftChange = useCallback((next: LoggedWorkout) => setDraft(next), [setDraft]);

    function handleDraftSaved(saved: LoggedWorkout) {
        setDraft(null);
        setLogs(prev => [saved, ...prev].sort(newestFirst));
        setSelectedId(saved.id);
        showToast({ message: `“${saved.name}” saved to your logbook` });
    }

    // "Log workout" on a template page lands here with the template id.
    useEffect(() => {
        const templateId = navState?.startTemplateId;
        if (!templateId || loading || handledTemplateStart.current === location.key) return;
        handledTemplateStart.current = location.key;
        getTemplates().then(ts => {
            const template = ts.find(t => t.id === templateId);
            if (template) handleStartFromTemplate(template);
            navigate(location.pathname, { replace: true, state: { selectedLogId: template ? DRAFT_ID : undefined } });
        });
    }, [loading, location.key]); // eslint-disable-line react-hooks/exhaustive-deps

    // ── Delete with undo ──────────────────────────────────────────────────────

    async function handleDelete(id: string) {
        if (isDraftId(id)) {
            const discarded = draft;
            setDraft(null);
            setSelectedId(null);
            if (discarded) {
                showToast({
                    message: "Draft discarded",
                    actionLabel: "Undo",
                    onAction: () => { setDraft(discarded); setSelectedId(DRAFT_ID); },
                });
            }
            return;
        }

        const removed = logs.find(l => l.id === id);
        if (!removed) return;
        setLogs(prev => prev.filter(l => l.id !== id));
        if (selectedId === id) setSelectedId(null);
        try {
            await deleteLog(id);
        } catch {
            setLogs(prev => [removed, ...prev].sort(newestFirst));
            showToast({ message: "Couldn't delete that log — it's back in your list." });
            return;
        }
        showToast({
            message: `“${removed.name}” deleted`,
            actionLabel: "Undo",
            onAction: async () => {
                // The server copy is gone, so undo re-creates it with the same content.
                const newId = await createLog(removed.name, removed.exercises, removed.note, removed.loggedAt);
                const restored = { ...removed, id: newId };
                setLogs(prev => [restored, ...prev].sort(newestFirst));
                setSelectedId(newId);
            },
        });
    }

    async function handleCreateTemplate(name: string, logExercises: LoggedExercise[]) {
        const templateExercises: TemplateExercise[] = [...logExercises]
            .sort((a, b) => a.order - b.order)
            .map(ex => ({
                exerciseId: ex.exerciseId,
                order: ex.order,
                note: ex.note ?? null,
                gear: ex.gear ?? [],
                sets: [...ex.sets].sort((a, b) => a.order - b.order).map((s, j) => {
                    if (s.type === "Cardio") {
                        return { type: "Cardio" as const, order: j + 1, reps: null,
                            duration: s.duration, distance: s.distance, distanceUnit: s.distanceUnit };
                    }
                    return { type: s.type as PlannedSet["type"], order: j + 1, reps: s.reps,
                        duration: null, distance: null, distanceUnit: null };
                }),
            }));
        await createTemplate(name || "Untitled template", templateExercises);
        navigate("/templates");
    }

    // ── Unsaved-changes guard ─────────────────────────────────────────────────

    const handleDirtyChange = useCallback((isDirty: boolean, save: () => Promise<void>) => {
        if (isDirty) {
            const log = selectedLogRef.current;
            setDirtyInfo({
                save,
                name: log?.name ?? "",
                date: log ? new Date(log.loggedAt).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" }) : "",
            });
        } else {
            setDirtyInfo(null);
        }
    }, []);

    // Stable between edits so the memoised sidebar doesn't re-render all its cards.
    const handleSelect = useCallback((id: string) => {
        if (dirtyInfo && id !== selectedId) {
            setPendingAction({ type: 'select', id });
        } else {
            setSelectedId(id);
        }
    }, [dirtyInfo, selectedId]);

    function handleBack() {
        if (dirtyInfo) {
            setPendingAction({ type: 'back' });
        } else {
            setSelectedId(null);
        }
    }

    async function handleDialogSave() {
        setDialogSaving(true);
        try {
            await dirtyInfo?.save();
            proceed();
        } finally {
            setDialogSaving(false);
        }
    }

    function handleDialogDiscard() {
        setDirtyInfo(null);
        proceed();
    }

    function handleDialogCancel() {
        if (blocker.state === "blocked") blocker.reset();
        setPendingAction(null);
    }

    function proceed() {
        if (blocker.state === "blocked") blocker.proceed();
        if (pendingAction?.type === 'select') setSelectedId(pendingAction.id);
        if (pendingAction?.type === 'back') setSelectedId(null);
        setPendingAction(null);
        setDirtyInfo(null);
    }

    const showDialog = pendingAction !== null || blocker.state === "blocked";

    useShortcut("k", () => { if (!startOpen) setStartOpen(true); }, true);

    return (
        <div className="panel-page logbook-page">
            {showDialog && dirtyInfo && (
                <UnsavedChangesDialog
                    name={dirtyInfo.name}
                    date={dirtyInfo.date}
                    saving={dialogSaving}
                    onSave={handleDialogSave}
                    onDiscard={handleDialogDiscard}
                    onCancel={handleDialogCancel}
                />
            )}
            {exportOpen && <ExportOverlay onClose={() => setExportOpen(false)} />}
            {startOpen && (
                <StartSessionOverlay
                    draft={draft}
                    onFromTemplate={handleStartFromTemplate}
                    onBlank={handleStartBlank}
                    onResumeDraft={handleResumeDraft}
                    onClose={() => setStartOpen(false)}
                />
            )}
            <HeaderComponent
                title="Logbook"
                action={
                    <div className="header-action-group">
                        <button
                            type="button"
                            className="header-icon-btn"
                            aria-label="Export logs"
                            title="Export logs"
                            onClick={() => setExportOpen(true)}
                        >
                            <Download size={15} />
                        </button>
                        <button
                            type="button"
                            className="header-icon-btn"
                            aria-label="Import logs"
                            title="Import logs"
                            onClick={() => navigate("/import")}
                        >
                            <Upload size={15} />
                        </button>
                        <Button text="Log workout" style="green hide-on-phone" icon={<Plus size={14} />} onClick={() => setStartOpen(true)} />
                    </div>
                }
                shortcuts={[
                    { keys: "Ctrl+K", label: "New log" },
                    { keys: "Ctrl+E", label: "Add exercise" },
                    { keys: "Ctrl+S", label: "Save" },
                    { keys: "Ctrl+↵", label: "Add set to focused exercise" },
                ]}
            />
            <div className={`panel-body${selectedId ? " has-selection" : ""}`}>
                <LogSideBar
                    logs={logs}
                    draft={draft}
                    selectedId={selectedId}
                    loading={loading}
                    onSelect={handleSelect}
                />
                <div className="panel-detail">
                    <button className="panel-back-btn" onClick={handleBack}>
                        <ChevronLeft size={16} /> Logbook
                    </button>
                    {selectedLog ? (
                        <LogDetail
                            key={isDraftId(selectedLog.id) ? newlyCreatedId ?? DRAFT_ID : selectedLog.id}
                            log={selectedLog}
                            exercises={exercises}
                            onSave={handleSave}
                            onDelete={handleDelete}
                            onCreateTemplate={handleCreateTemplate}
                            onDirtyChange={handleDirtyChange}
                            onExerciseCreated={handleExerciseCreated}
                            userGear={userGear}
                            autoFocusTitle={isDraftId(selectedLog.id) && !selectedLog.name}
                            openMap={openMap}
                            isDraft={isDraftId(selectedLog.id)}
                            onDraftChange={handleDraftChange}
                            onDraftSaved={handleDraftSaved}
                            previousSets={previousSets}
                        />
                    ) : selectedId && loading ? (
                        <DetailSkeleton />
                    ) : (
                        <div className="panel-empty">
                            <p>Select a log, or start a new one.</p>
                            <Button text="Log workout" style="green" icon={<Plus size={14} />} onClick={() => setStartOpen(true)} />
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default LogbookPage;
