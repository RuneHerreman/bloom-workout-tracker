import type { ReactNode } from "react";
import { X } from "lucide-react";

interface OverlayProps {
    title: string;
    subtitle: string;
    children?: ReactNode;
    onClose: () => void;
    noPadding?: boolean;
    panelClassName?: string;
}

function Overlay({ title, subtitle, children, onClose, noPadding, panelClassName }: OverlayProps) {
    return (
        <div className="overlay-backdrop" onClick={onClose}>
            <div className={`overlay-panel${panelClassName ? ` ${panelClassName}` : ""}`} onClick={e => e.stopPropagation()}>
                <header className="overlay-header">
                    <div>
                        <p className="general-widget-subtitle">{subtitle}</p>
                        <p className="general-widget-title">{title}</p>
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