import type { ReactNode } from "react";
import { Info } from "lucide-react";

interface Shortcut {
    keys: string;
    label: string;
}

interface HeaderComponentProps {
    title: string;
    /** Optional sentence-case context shown under the title (e.g. today's date). */
    subtitle?: string;
    action?: ReactNode;
    shortcuts?: Shortcut[];
}

function HeaderComponent({ title, subtitle, action, shortcuts }: HeaderComponentProps) {
    return (
        <header className="page-header">
            <div className="page-header-titles">
                <h1 className="header-title">{title}</h1>
                {subtitle && <p className="header-subtitle">{subtitle}</p>}
            </div>
            <div className="page-header-right">
                {shortcuts && (
                    <div className="shortcuts-info tooltip-anchor">
                        <Info size={15} />
                        <div className="tooltip">
                            {shortcuts.map(s => (
                                <div key={s.keys} className="shortcut-row">
                                    <kbd>{s.keys}</kbd>
                                    <span>{s.label}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                )}
                {action}
            </div>
        </header>
    );
}

export default HeaderComponent;