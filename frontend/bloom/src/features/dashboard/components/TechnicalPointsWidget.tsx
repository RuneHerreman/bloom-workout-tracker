import { useEffect, useRef } from "react";
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
            editorRef.current.innerHTML = initialContent;
        }
    }, [initialContent]);

    function execFormat(command: string, value?: string) {
        document.execCommand(command, false, value);
        editorRef.current?.focus();
    }

    function handleSave() {
        onSave?.(editorRef.current?.innerHTML ?? "");
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
                        <button onMouseDown={e => { e.preventDefault(); execFormat("bold"); }} title="Bold"><b>B</b></button>
                        <button onMouseDown={e => { e.preventDefault(); execFormat("italic"); }} title="Italic"><i>I</i></button>
                        <button onMouseDown={e => { e.preventDefault(); execFormat("underline"); }} title="Underline"><u>U</u></button>
                        <span className="technical-points-divider" />
                        <button onMouseDown={e => { e.preventDefault(); execFormat("insertUnorderedList"); }} title="Bullet list">≡</button>
                        <button onMouseDown={e => { e.preventDefault(); execFormat("insertOrderedList"); }} title="Numbered list">1.</button>
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
