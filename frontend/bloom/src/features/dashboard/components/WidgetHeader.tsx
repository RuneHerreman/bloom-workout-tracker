import type { ReactNode } from "react";

interface WidgetHeaderProps {
    title: string;
    /** Optional sentence-case clarification shown under the title — only when it adds information. */
    subtitle?: string;
    action?: ReactNode;
}

function WidgetHeader({ title, subtitle, action }: WidgetHeaderProps) {
    return (
        <>
            <div className="general-widget-titles">
                <h2 className="general-widget-title">{title}</h2>
                {subtitle && <p className="general-widget-desc">{subtitle}</p>}
            </div>
            {action}
        </>
    );
}

export default WidgetHeader;
