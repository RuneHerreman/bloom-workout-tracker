import { useEffect, useState } from "react";
import "../../assets/css/templates.css";
import { getTemplates, deleteTemplate } from "./api.ts";
import type { WorkoutTemplate, TemplateExercise } from "./api.ts";
import { searchExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import TemplateSideBar from "./components/TemplateSideBar.tsx";
import TemplateDetail from "./components/TemplateDetail.tsx";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import Button from "../../components/general/ButtonComponent.tsx";
import Overlay from "../../components/general/OverlayComponent.tsx";
import {PlusIcon} from "lucide-react";
import AddExerciseButton from "./components/AddExerciseButton.tsx";
import ExerciseLibrary from "./components/ExerciseLibrary.tsx";

const TemplatePage = () => {
    const [templates, setTemplates] = useState<WorkoutTemplate[]>([]);
    const [exercises, setExercises] = useState<Record<string, Exercise>>({});
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [addExerciseOpen, setAddExerciseOpen] = useState(false);

    useEffect(() => {
        Promise.all([getTemplates(), searchExercises()]).then(([tpls, exs]) => {
            setTemplates(tpls);
            setExercises(Object.fromEntries(exs.map(e => [e.id, e])));
            setLoading(false);
        });
    }, []);

    const selectedTemplate = templates.find(t => t.id === selectedId) ?? null;

    const handleSave = (id: string, exercises: TemplateExercise[]) => {
        setTemplates(prev => prev.map(t => t.id === id ? { ...t, exercises } : t));
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
        <div className="templates-page">
            <HeaderComponent title="Templates" subtitle="Library" action={<Button text={"New template"} icon={<PlusIcon size={15} />} style={"green"} />}/>
            {addExerciseOpen && (
                <Overlay title="Exercise library" subtitle="Add exercise" onClose={() => setAddExerciseOpen(false)}>
                    <ExerciseLibrary exercises={Object.values(exercises)} />
                </Overlay>
            )}
            <div className="templates-body">
                <TemplateSideBar
                    templates={templates}
                    selectedId={selectedId}
                    loading={loading}
                    onSelect={setSelectedId}
                    onDelete={handleDelete}
                />
                <div className="template-detail">
                    {selectedTemplate ? (<> <TemplateDetail key={selectedTemplate.id} template={selectedTemplate} exercises={exercises} onDelete={handleDelete} onSave={handleSave} /> <AddExerciseButton onClick={() => setAddExerciseOpen(true)} /> </>)
                        : (
                            <div className="template-detail-empty">
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
