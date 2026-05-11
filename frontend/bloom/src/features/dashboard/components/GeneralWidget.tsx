import type {ReactNode} from "react";

function GeneralWidget({content, header, className}: { content: ReactNode, header?: ReactNode, className?: string }) {
    return (
        <div className={`general-widget${className ? ` ${className}` : ""}`}>
            {header && <header className="general-widget-header">{header}</header>}
            <div className="general-widget-content">
                {content}
            </div>
        </div>
    );
}

export default GeneralWidget;