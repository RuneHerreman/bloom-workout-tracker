import { useEffect, useRef } from "react";
import DOMPurify from "dompurify";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";

interface TechnicalPointsWidgetProps {
    initialContent?: string | null;
    onSave?: (content: string) => void;
}

function TechnicalPointsWidget({ initialContent, onSave }: TechnicalPointsWidgetProps) {
    const editorRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (editorRef.current && initialContent != null) {
            editorRef.current.innerHTML = DOMPurify.sanitize(initialContent);
        }
    }, [initialContent]);

    function applyUnderline() {
        editorRef.current?.focus();
        const selection = window.getSelection();
        if (!selection || selection.rangeCount === 0 || selection.isCollapsed) return;

        const range = selection.getRangeAt(0);
        const fragment = range.extractContents();
        const u = document.createElement("u");
        u.appendChild(fragment);
        range.insertNode(u);

        // Restore selection around the new element
        const newRange = document.createRange();
        newRange.selectNodeContents(u);
        selection.removeAllRanges();
        selection.addRange(newRange);
    }

    function handleSave() {
        const raw = editorRef.current?.innerHTML ?? "";
        onSave?.(DOMPurify.sanitize(raw));
    }

    return (
        <GeneralWidget
            header={
                <WidgetHeader
                    title={"Technical Points"}
                    subtitle={"What needs work?"}
                    action={<Button text={"Save notes"} style={"modern"} onClick={handleSave}/>}
                />
            }
            content={
                <div className="technical-points">
                    <div className="technical-points-toolbar">
                        <button
                            onMouseDown={e => { e.preventDefault(); applyUnderline(); }}
                            title="Underline"
                        >
                            <u>U</u>
                        </button>
                    </div>
                    <div
                        ref={editorRef}
                        className="technical-points-editor"
                        contentEditable
                        suppressContentEditableWarning
                        data-placeholder="Add technical notes..."
                    />
                </div>
            }
        />
    );
}

export default TechnicalPointsWidget;
