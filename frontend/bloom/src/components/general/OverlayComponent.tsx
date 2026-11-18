import { useId, useRef, type ReactNode } from "react";
import { useFocusTrap, focusables } from "../../hooks/useFocusTrap.ts";
import { X } from "lucide-react";

interface OverlayProps {
    title: string;
    subtitle?: string;
    children?: ReactNode;
    onClose: () => void;
    noPadding?: boolean;
    panelClassName?: string;
}

function Overlay({ title, subtitle, children, onClose, noPadding, panelClassName }: OverlayProps) {
    const panelRef = useRef<HTMLDivElement>(null);
    const titleId = useId();
    // Start on the first control in the body, not the close button in the header.
    useFocusTrap(panelRef, onClose, root => {
        const content = root.querySelector<HTMLElement>(".overlay-content");
        return content ? focusables(content)[0] : null;
    });
    return (
        <div className="overlay-backdrop" onClick={onClose}>
            <div
                ref={panelRef}
                className={`overlay-panel${panelClassName ? ` ${panelClassName}` : ""}`}
                role="dialog"
                aria-modal="true"
                aria-labelledby={titleId}
                tabIndex={-1}
                onClick={e => e.stopPropagation()}
            >
                <header className="overlay-header">
                    <div>
                        <h2 id={titleId} className="overlay-title">{title}</h2>
                        {subtitle && <p className="overlay-subtitle">{subtitle}</p>}
                    </div>
                    <button className="overlay-close" onClick={onClose} aria-label="Close">
                        <X size={16} />
                    </button>
                </header>
                <div className={`overlay-content${noPadding ? " overlay-content-no-padding" : ""}`}>
                    {children}
                </div>
            </div>
        </div>
    );
}

export default Overlay;