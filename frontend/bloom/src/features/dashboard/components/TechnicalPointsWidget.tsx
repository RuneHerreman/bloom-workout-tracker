import { useEffect, useRef, useState } from "react";
import DOMPurify from "dompurify";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";

interface TechnicalPointsWidgetProps {
    initialContent?: string | null;
    onSave?: (content: string) => Promise<unknown>;
}

type SaveState = "idle" | "dirty" | "saving" | "saved" | "error";

const SAVE_LABEL: Record<SaveState, string> = {
    idle: "", dirty: "Unsaved changes", saving: "Saving…", saved: "Saved", error: "Couldn't save, try again",
};

/** Free-form technique cues. Saves by itself a moment after you stop typing, and when you click away. */
function TechnicalPointsWidget({ initialContent, onSave }: TechnicalPointsWidgetProps) {
    const editorRef = useRef<HTMLDivElement>(null);
    const timer = useRef<number | undefined>(undefined);
    const [state, setState] = useState<SaveState>("idle");

    useEffect(() => {
        // Don't clobber what's being typed when the user record refreshes.
        if (editorRef.current && initialContent != null && document.activeElement !== editorRef.current) {
            editorRef.current.innerHTML = DOMPurify.sanitize(initialContent);
        }
    }, [initialContent]);

    useEffect(() => () => window.clearTimeout(timer.current), []);

    function save() {
        window.clearTimeout(timer.current);
        if (!onSave) return;
        setState("saving");
        onSave(DOMPurify.sanitize(editorRef.current?.innerHTML ?? ""))
            .then(() => setState("saved"))
            .catch(() => setState("error"));
    }

    function handleInput() {
        setState("dirty");
        window.clearTimeout(timer.current);
        timer.current = window.setTimeout(save, 1200);
    }

    function applyUnderline() {
        editorRef.current?.focus();
        const selection = window.getSelection();
        if (!selection || selection.rangeCount === 0 || selection.isCollapsed) return;

        const range = selection.getRangeAt(0);
        const fragment = range.extractContents();
        const u = document.createElement("u");
        u.appendChild(fragment);
        range.insertNode(u);

        const newRange = document.createRange();
        newRange.selectNodeContents(u);
        selection.removeAllRanges();
        selection.addRange(newRange);
        handleInput();
    }

    return (
        <GeneralWidget
            className="notes-widget"
            header={
                <WidgetHeader
                    title="Technique notes"
                    subtitle="Cues to keep in mind while you train"
                    action={<span className={`notes-status ${state}`} aria-live="polite">{SAVE_LABEL[state]}</span>}
                />
            }
            content={
                <div className="technical-points">
                    <div className="technical-points-toolbar">
                        <button onMouseDown={e => { e.preventDefault(); applyUnderline(); }} title="Underline selection" aria-label="Underline selection">
                            <u>U</u>
                        </button>
                    </div>
                    <div
                        ref={editorRef}
                        className="technical-points-editor"
                        contentEditable
                        suppressContentEditableWarning
                        role="textbox"
                        aria-multiline="true"
                        aria-label="Technique notes"
                        data-placeholder="e.g. Bench: shoulder blades back, feet planted. Squat: brace before every rep."
                        onInput={handleInput}
                        onBlur={() => state === "dirty" && save()}
                    />
                </div>
            }
        />
    );
}

export default TechnicalPointsWidget;
