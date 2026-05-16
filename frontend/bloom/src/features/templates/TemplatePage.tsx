import { useEffect, useMemo, useState } from "react";
import { useLocation } from "react-router-dom";
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
import { PlusIcon, ChevronLeft } from "lucide-react";
import AddExerciseButton from "./components/AddExerciseButton.tsx";
import ExerciseLibrary from "./components/ExerciseLibrary.tsx";

const TemplatePage = () => {
    const location = useLocation();
    const [templates, setTemplates] = useState<WorkoutTemplate[]>([]);
    const [exercises, setExercises] = useState<Record<string, Exercise>>({});
    const [selectedId, setSelectedId] = useState<string | null>(null);

    useEffect(() => { setSelectedId(null); }, [location.key]);
    const [loading, setLoading] = useState(true);
    const [addExerciseOpen, setAddExerciseOpen] = useState(false);
    const [creating, setCreating] = useState(false);
    const [pendingExerciseId, setPendingExerciseId] = useState<string | null>(null);

    useEffect(() => {
        Promise.all([getTemplates(), searchExercises()]).then(([tpls, exs]) => {
            setTemplates(tpls);
            setExercises(Object.fromEntries(exs.map(e => [e.id, e])));
            setLoading(false);
        });
    }, []);

    const selectedTemplate = useMemo(() => templates.find(t => t.id === selectedId) ?? null, [templates, selectedId]);

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

    return (
        <div className="panel-page">
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
                    onSelect={setSelectedId}
                />
                <div className="panel-detail">
                    <button className="panel-back-btn" onClick={() => setSelectedId(null)}>
                        <ChevronLeft size={16} /> Templates
                    </button>
                    {selectedTemplate ? (<> <TemplateDetail key={selectedTemplate.id} template={selectedTemplate} exercises={exercises} onDelete={handleDelete} onSave={handleSave} pendingExerciseId={pendingExerciseId} onExerciseAdded={() => setPendingExerciseId(null)} /> <AddExerciseButton onClick={() => setAddExerciseOpen(true)} /> </>)
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
