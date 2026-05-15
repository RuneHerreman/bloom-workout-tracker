import type { ReactNode } from "react";
import { NavLink } from 'react-router-dom';

interface ButtonComponentProps {
    text: string;
    icon?: ReactNode;
    target?: string;
    style: string;
    onClick?: () => void;
    disabled?: boolean;
}

function Button({ text, icon, target, style, onClick, disabled }: ButtonComponentProps) {
    if (onClick || disabled) {
        return (
            <button type="button" className={`button-component ${style ?? ""}`} onClick={onClick} disabled={disabled}>
                {icon}
                {text}
            </button>
        );
    }

    return (
        <NavLink to={target ?? "#"} className={`button-component ${style ?? ""}`}>
            {icon}
            {text}
        </NavLink>
    );
}

export default Button;