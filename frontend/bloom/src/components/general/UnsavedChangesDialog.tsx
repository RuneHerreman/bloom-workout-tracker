import Overlay from "./OverlayComponent.tsx";

interface UnsavedChangesDialogProps {
    name: string;
    date?: string;
    saving: boolean;
    onSave: () => void;
    onDiscard: () => void;
    onCancel: () => void;
}

function UnsavedChangesDialog({ name, date, saving, onSave, onDiscard, onCancel }: UnsavedChangesDialogProps) {
    const label = date ? `${name}, ${date}` : name;

    return (
        <Overlay title="Unsaved changes" subtitle="Hold on" onClose={onCancel}>
            <div className="unsaved-dialog">
                <p className="unsaved-dialog-body">
                    You have unsaved changes for <strong>{label}</strong>
                </p>
                <div className="unsaved-dialog-actions">
                    <button className="unsaved-dialog-discard" onClick={onDiscard}>I don't care</button>
                    <button className="unsaved-dialog-save" onClick={onSave} disabled={saving}>
                        {saving ? "Saving…" : "Save"}
                    </button>
                </div>
            </div>
        </Overlay>
    );
}

export default UnsavedChangesDialog;
