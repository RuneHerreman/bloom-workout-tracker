import type {ReactNode} from "react";

interface WidgetHeaderProps {
    title: string;
    subtitle: string;
    action?: ReactNode
}

function WidgetHeader({ title, subtitle, action }: WidgetHeaderProps) {
    return (
        <>
            <div>
                <p className="general-widget-subtitle">{subtitle}</p>
                <p className="general-widget-title">{title}</p>
            </div>
            {action}
        </>
    );
}

export default WidgetHeader;