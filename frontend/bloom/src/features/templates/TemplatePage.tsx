import { useEffect, useMemo, useState, useCallback, useRef } from "react";
import { useLocation, useBlocker } from "react-router-dom";
import "../../assets/css/templates.css";
import { getTemplates, getTemplate, createTemplate, deleteTemplate } from "./api.ts";
import type { WorkoutTemplate, TemplateExercise } from "./api.ts";
import { searchExercises } from "../exercises/api.ts";
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

const TemplatePage = () => {
    const location = useLocation();
    const [templates, setTemplates] = useState<WorkoutTemplate[]>([]);
    const [exercises, setExercises] = useState<Record<string, Exercise>>({});
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [dirtyInfo, setDirtyInfo] = useState<{ save: () => Promise<void>; name: string } | null>(null);
    const [pendingAction, setPendingAction] = useState<{ type: 'select'; id: string } | { type: 'back' } | null>(null);
    const [dialogSaving, setDialogSaving] = useState(false);
    const selectedTemplateRef = useRef<typeof selectedTemplate>(null);

    const blocker = useBlocker(() => dirtyInfo !== null);

    useEffect(() => { setSelectedId(null); }, [location.key]);
    const [loading, setLoading] = useState(true);
    const [addExerciseOpen, setAddExerciseOpen] = useState(false);
    const [creating, setCreating] = useState(false);
    const [pendingExerciseId, setPendingExerciseId] = useState<string | null>(null);

    useEffect(() => {
        getTemplates().then(tpls => {
            setTemplates(tpls);
            setLoading(false);
        });
        searchExercises().then(exs => {
            setExercises(Object.fromEntries(exs.map(e => [e.id, e])));
        });
    }, []);

    const selectedTemplate = useMemo(() => templates.find(t => t.id === selectedId) ?? null, [templates, selectedId]);
    selectedTemplateRef.current = selectedTemplate;

    const handleSave = (id: string, name: string, exercises: TemplateExercise[]) => {
        setTemplates(prev => prev.map(t => t.id === id ? { ...t, name, exercises } : t));
    };

    const handleNewTemplate = async () => {
        setCreating(true);
        try {
            const benchPress = Object.values(exercises).find(e => e.name.toLowerCase().includes("bench press"));
            const defaultExercises: TemplateExercise[] = benchPress ? [{
                exerciseId: benchPress.id,
                order: 1,
                sets: [{ type: "Strength", order: 1, reps: 10, duration: null, distance: null, distanceUnit: null }],
            }] : [];
            const newId = await createTemplate("Untitled template", defaultExercises);
            const newTemplate = await getTemplate(newId);
            setTemplates(prev => [newTemplate, ...prev]);
            setSelectedId(newId);
        } finally {
            setCreating(false);
        }
    };

    const handleDelete = async (id: string) => {
        setTemplates(prev => prev.filter(t => t.id !== id));
        if (selectedId === id) setSelectedId(null);
        try {
            await deleteTemplate(id);
        } catch {
            const tpls = await getTemplates();
            setTemplates(tpls);
        }
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
            <HeaderComponent title="Templates" subtitle="Library" action={<Button text={"New template"} icon={<PlusIcon size={15} />} style={"green"} onClick={handleNewTemplate} disabled={creating} />}/>
            {addExerciseOpen && (
                <Overlay title="Exercise library" subtitle="Add exercise" onClose={() => setAddExerciseOpen(false)}>
                    <ExerciseLibrary
                        exercises={Object.values(exercises)}
                        onSelect={e => {
                            setPendingExerciseId(e.id);
                            setAddExerciseOpen(false);
                        }}
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
                    {selectedTemplate ? (<> <TemplateDetail key={selectedTemplate.id} template={selectedTemplate} exercises={exercises} onDelete={handleDelete} onSave={handleSave} pendingExerciseId={pendingExerciseId} onExerciseAdded={() => setPendingExerciseId(null)} onDirtyChange={handleDirtyChange} /> <AddExerciseButton onClick={() => setAddExerciseOpen(true)} /> </>)
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
