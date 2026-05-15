import type { ReactNode } from "react";
import { X } from "lucide-react";

interface OverlayProps {
    title: string;
    subtitle: string;
    children?: ReactNode;
    onClose: () => void;
}

function Overlay({ title, subtitle, children, onClose }: OverlayProps) {
    return (
        <div className="overlay-backdrop" onClick={onClose}>
            <div className="overlay-panel" onClick={e => e.stopPropagation()}>
                <header className="overlay-header">
                    <div>
                        <p className="general-widget-subtitle">{subtitle}</p>
                        <p className="general-widget-title">{title}</p>
                    </div>
                    <button className="overlay-close" onClick={onClose} aria-label="Close">
                        <X size={16} />
                    </button>
                </header>
                <div className="overlay-content">
                    {children}
                </div>
            </div>
        </div>
    );
}

export default Overlay;