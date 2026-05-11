import type {ReactNode} from "react";

interface HeaderComponentProps {
    title: string;
    subtitle: string;
    action?: ReactNode
}

function HeaderComponent({ title, subtitle, action }: HeaderComponentProps) {
    return (
        <header className="page-header">
            <div>
                <p className="header-subtitle">{subtitle}</p>
                <h1 className="header-title">{title}</h1>
            </div>
            {action}
        </header>
    );
}

export default HeaderComponent;