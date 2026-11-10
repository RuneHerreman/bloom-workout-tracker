import { useEffect, useLayoutEffect, useRef } from "react";
import { StickyNote, X } from "lucide-react";

interface ExerciseNoteProps {
    value: string;
    onChange: (value: string) => void;
    onClose: () => void;
    placeholder?: string;
    autoFocus?: boolean;
}

/** Auto-growing note field shown between an exercise card's header and its sets. */
function ExerciseNote({ value, onChange, onClose, placeholder = "Cues, setup, how it felt…", autoFocus }: ExerciseNoteProps) {
    const ref = useRef<HTMLTextAreaElement>(null);

    useLayoutEffect(() => {
        const el = ref.current;
        if (!el) return;
        el.style.height = "auto";
        el.style.height = `${el.scrollHeight}px`;
    }, [value]);

    useEffect(() => {
        if (autoFocus) ref.current?.focus();
    }, [autoFocus]);

    return (
        <div className="exercise-note">
            <StickyNote size={13} className="exercise-note-icon" aria-hidden />
            <textarea
                ref={ref}
                rows={1}
                maxLength={2000}
                value={value}
                placeholder={placeholder}
                aria-label="Exercise note"
                onChange={e => onChange(e.target.value)}
                onKeyDown={e => {
                    if (e.key === "Escape" && value.trim() === "") onClose();
                }}
            />
            <button
                type="button"
                className="exercise-note-remove"
                tabIndex={-1}
                aria-label="Remove note"
                onClick={() => { onChange(""); onClose(); }}
            >
                <X size={12} />
            </button>
        </div>
    );
}

export default ExerciseNote;
