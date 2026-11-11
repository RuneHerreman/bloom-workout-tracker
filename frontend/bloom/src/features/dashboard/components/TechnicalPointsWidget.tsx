import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import DOMPurify from "dompurify";
import { Bold, Italic, List, Underline } from "lucide-react";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";

interface TechnicalPointsWidgetProps {
    initialContent?: string | null;
    onSave?: (content: string) => Promise<unknown>;
}

type SaveState = "idle" | "dirty" | "saving" | "saved" | "error";
type FormatCommand = "bold" | "italic" | "underline" | "insertUnorderedList";

const isMac = typeof navigator !== "undefined" && /Mac|iPhone|iPad/.test(navigator.platform);
const MOD = isMac ? "⌘" : "Ctrl";

const FORMATS: { cmd: FormatCommand; label: string; key: string; shift: boolean; hint: string; icon: typeof Bold }[] = [
    { cmd: "bold", label: "Bold", key: "b", shift: false, hint: `${MOD}+B`, icon: Bold },
    { cmd: "italic", label: "Italic", key: "i", shift: false, hint: `${MOD}+I`, icon: Italic },
    { cmd: "underline", label: "Underline", key: "u", shift: false, hint: `${MOD}+U`, icon: Underline },
    { cmd: "insertUnorderedList", label: "Bulleted list", key: "8", shift: true, hint: `${MOD}+Shift+8`, icon: List },
];

const SAVE_LABEL: Record<SaveState, string> = {
    idle: "", dirty: "Unsaved changes", saving: "Saving…", saved: "Saved", error: "Couldn't save, try again",
};

/** Free-form technique cues. Saves by itself a moment after you stop typing, and when you click away. */
function TechnicalPointsWidget({ initialContent, onSave }: TechnicalPointsWidgetProps) {
    const editorRef = useRef<HTMLDivElement>(null);
    const timer = useRef<number | undefined>(undefined);
    const [state, setState] = useState<SaveState>("idle");
    const [active, setActive] = useState<Set<FormatCommand>>(new Set());

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

    // execCommand is deprecated but still the only cross-browser way to toggle inline
    // formatting in a contentEditable while keeping the native undo stack intact.
    function format(cmd: FormatCommand) {
        editorRef.current?.focus();
        document.execCommand(cmd);
        refreshActive();
        handleInput();
    }

    function refreshActive() {
        const sel = window.getSelection();
        if (!sel || !editorRef.current?.contains(sel.anchorNode)) return;
        setActive(new Set(FORMATS.filter(f => document.queryCommandState(f.cmd)).map(f => f.cmd)));
    }

    useEffect(() => {
        document.addEventListener("selectionchange", refreshActive);
        return () => document.removeEventListener("selectionchange", refreshActive);
    }, []);

    function handleKeyDown(e: KeyboardEvent) {
        if (!(e.ctrlKey || e.metaKey)) return;
        const key = e.key.toLowerCase();
        const hit = FORMATS.find(f => f.key === key && f.shift === e.shiftKey);
        if (!hit) return;
        e.preventDefault();
        format(hit.cmd);
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
                    <div className="technical-points-toolbar" role="toolbar" aria-label="Formatting">
                        {FORMATS.map(({ cmd, label, hint, icon: Icon }) => (
                            <button
                                key={cmd}
                                type="button"
                                className={active.has(cmd) ? "active" : undefined}
                                aria-pressed={active.has(cmd)}
                                onMouseDown={e => { e.preventDefault(); format(cmd); }}
                                title={`${label} (${hint})`}
                                aria-label={label}
                            >
                                <Icon size={14} strokeWidth={2.25} />
                            </button>
                        ))}
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
                        onKeyDown={handleKeyDown}
                        onBlur={() => state === "dirty" && save()}
                    />
                </div>
            }
        />
    );
}

export default TechnicalPointsWidget;
