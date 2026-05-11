import { useEffect, useState } from "react";
import "../../assets/css/templates.css";
import { getTemplates, deleteTemplate } from "./api.ts";
import type { WorkoutTemplate } from "./api.ts";
import TemplateSideBar from "./components/TemplateSideBar.tsx";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";

const TemplatePage = () => {
    const [templates, setTemplates] = useState<WorkoutTemplate[]>([]);
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getTemplates().then(tpls => {
            setTemplates(tpls);
            setLoading(false);
        });
    }, []);

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
            <HeaderComponent title="Templates" subtitle="Library" />
            <div className="templates-body">
                <TemplateSideBar
                    templates={templates}
                    selectedId={selectedId}
                    loading={loading}
                    onSelect={setSelectedId}
                    onDelete={handleDelete}
                />
                <div className="template-detail">
                    {!selectedId && (
                        <div className="template-detail-empty">
                            <p>Select a template to see details</p>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default TemplatePage;
