import { useEffect, useMemo, useState, useCallback, useRef } from "react";
import { useLocation, useBlocker, useNavigate } from "react-router-dom";
import "../../assets/css/templates.css";
import { getTemplates, getTemplate, createTemplate, deleteTemplate, peekTemplates } from "./api.ts";
import type { WorkoutTemplate, TemplateExercise } from "./api.ts";
import { searchExercises, peekExercises } from "../exercises/api.ts";
import { getMe, peekMe } from "../auth/api.ts";
import type { Exercise } from "../exercises/api.ts";
import TemplateSideBar from "./components/TemplateSideBar.tsx";
import TemplateDetail from "./components/TemplateDetail.tsx";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import Button from "../../components/general/ButtonComponent.tsx";
import Overlay from "../../components/general/OverlayComponent.tsx";
import UnsavedChangesDialog from "../../components/general/UnsavedChangesDialog.tsx";
import { PlusIcon, ChevronLeft } from "lucide-react";
import AddExerciseButton from "./components/AddExerciseButton.tsx";
import ExerciseLibrary from "./components/ExerciseLibrary.tsx";
import { useShortcut } from "../../hooks/useShortcut.ts";
import { useToast } from "../../context/ToastContext.tsx";

const TemplatePage = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { showToast } = useToast();
    const [templates, setTemplates] = useState<WorkoutTemplate[]>(() => peekTemplates() ?? []);
    const [exercises, setExercises] = useState<Record<string, Exercise>>(
        () => Object.fromEntries((peekExercises() ?? []).map(e => [e.id, e]))
    );
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [dirtyInfo, setDirtyInfo] = useState<{ save: () => Promise<void>; name: string } | null>(null);
    const [pendingAction, setPendingAction] = useState<{ type: 'select'; id: string } | { type: 'back' } | null>(null);
    const [dialogSaving, setDialogSaving] = useState(false);
    const selectedTemplateRef = useRef<typeof selectedTemplate>(null);

    const blocker = useBlocker(() => dirtyInfo !== null);

    useEffect(() => { setSelectedId(null); }, [location.key]);
    const [loading, setLoading] = useState(() => peekTemplates() === undefined);
    const [addExerciseOpen, setAddExerciseOpen] = useState(false);
    const [creating, setCreating] = useState(false);
    const [pendingExerciseId, setPendingExerciseId] = useState<string | null>(null);
    const [newlyCreatedId, setNewlyCreatedId] = useState<string | null>(null);
    const [userGear, setUserGear] = useState<string[]>(() => peekMe()?.gear ?? []);

    useEffect(() => {
        getTemplates().then(tpls => {
            setTemplates(tpls);
            setLoading(false);
        });
        searchExercises().then(exs => {
            setExercises(Object.fromEntries(exs.map(e => [e.id, e])));
        });
        getMe().then(u => setUserGear(u.gear ?? [])).catch(() => null);
    }, []);

    const exerciseList = useMemo(() => Object.values(exercises), [exercises]);
    const selectedTemplate = useMemo(() => templates.find(t => t.id === selectedId) ?? null, [templates, selectedId]);
    selectedTemplateRef.current = selectedTemplate;

    const handleSave = (id: string, name: string, exercises: TemplateExercise[]) => {
        setTemplates(prev => prev.map(t => t.id === id ? { ...t, name, exercises } : t));
    };

    // New templates start empty with the exercise library already open.
    const handleNewTemplate = async () => {
        if (creating) return;
        setCreating(true);
        try {
            const newId = await createTemplate("Untitled template", []);
            const newTemplate = await getTemplate(newId);
            setTemplates(prev => [newTemplate, ...prev]);
            setSelectedId(newId);
            setNewlyCreatedId(newId);
            setAddExerciseOpen(true);
        } finally {
            setCreating(false);
        }
    };

    const handleDelete = async (id: string) => {
        const removed = templates.find(t => t.id === id);
        if (!removed) return;
        setTemplates(prev => prev.filter(t => t.id !== id));
        if (selectedId === id) setSelectedId(null);
        try {
            await deleteTemplate(id);
        } catch {
            setTemplates(await getTemplates());
            showToast({ message: "Couldn't delete that template — it's back in your list." });
            return;
        }
        showToast({
            message: `“${removed.name}” deleted`,
            actionLabel: "Undo",
            onAction: async () => {
                const newId = await createTemplate(removed.name, removed.exercises);
                setTemplates(prev => [{ ...removed, id: newId }, ...prev]);
                setSelectedId(newId);
            },
        });
    };

    const handleLogWorkout = (templateId: string) => {
        navigate("/logbook", { state: { startTemplateId: templateId } });
    };

    const handleDirtyChange = useCallback((isDirty: boolean, save: () => Promise<void>) => {
        if (isDirty) {
            setDirtyInfo({ save, name: selectedTemplateRef.current?.name ?? "" });
        } else {
            setDirtyInfo(null);
        }
    }, []);

    function handleSelect(id: string) {
        if (dirtyInfo && id !== selectedId) {
            setPendingAction({ type: 'select', id });
        } else {
            setSelectedId(id);
        }
    }

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

    useShortcut("k", () => { if (!creating) handleNewTemplate(); }, true);
    useShortcut("e", () => { if (selectedId && !addExerciseOpen) setAddExerciseOpen(true); }, true);

    return (
        <div className="panel-page">
            {showDialog && dirtyInfo && (
                <UnsavedChangesDialog
                    name={dirtyInfo.name}
                    saving={dialogSaving}
                    onSave={handleDialogSave}
                    onDiscard={handleDialogDiscard}
                    onCancel={handleDialogCancel}
                />
            )}
            <HeaderComponent
                title="Templates"
                action={<Button text={"New template"} icon={<PlusIcon size={15} />} style={"green"} onClick={handleNewTemplate} disabled={creating} />}
                shortcuts={[
                    { keys: "Ctrl+K", label: "New template" },
                    { keys: "Ctrl+E", label: "Add exercise" },
                    { keys: "Ctrl+S", label: "Save" },
                    { keys: "Ctrl+↵", label: "Add set to focused exercise" },
                ]}
            />
            {addExerciseOpen && (
                <Overlay title="Add exercise" panelClassName="overlay-panel-fixed" onClose={() => setAddExerciseOpen(false)}>
                    <ExerciseLibrary
                        exercises={exerciseList}
                        onSelect={e => {
                            setPendingExerciseId(e.id);
                            setAddExerciseOpen(false);
                        }}
                        onSaved={e => setExercises(prev => ({ ...prev, [e.id]: e }))}
                    />
                </Overlay>
            )}
            <div className={`panel-body${selectedId ? " has-selection" : ""}`}>
                <TemplateSideBar
                    templates={templates}
                    selectedId={selectedId}
                    loading={loading}
                    onSelect={handleSelect}
                />
                <div className="panel-detail">
                    <button className="panel-back-btn" onClick={handleBack}>
                        <ChevronLeft size={16} /> Templates
                    </button>
                    {selectedTemplate ? (<> <TemplateDetail key={selectedTemplate.id} template={selectedTemplate} exercises={exercises} onDelete={handleDelete} onSave={handleSave} pendingExerciseId={pendingExerciseId} onExerciseAdded={() => setPendingExerciseId(null)} onDirtyChange={handleDirtyChange} onLogWorkout={handleLogWorkout} userGear={userGear} autoFocusTitle={selectedTemplate.id === newlyCreatedId} /> <AddExerciseButton onClick={() => setAddExerciseOpen(true)} /> </>)
                        : (
                            <div className="panel-empty">
                                <p>Select a template to see details</p>
                            </div>
                        )
                    }
                </div>
            </div>
        </div>
    );
};

export default TemplatePage;
