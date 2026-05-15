import type { ReactNode } from "react";
import { NavLink } from 'react-router-dom';

interface ButtonComponentProps {
    text: string;
    icon?: ReactNode;
    imageSrc?: string;
    target?: string;
    style: string;
    onClick?: () => void;
    disabled?: boolean;
}

function Button({ text, icon, imageSrc, target, style, onClick, disabled }: ButtonComponentProps) {
    if (onClick || disabled) {
        return (
            <button type="button" className={`button-component ${style ?? ""}`} onClick={onClick} disabled={disabled}>
                {icon}
                {imageSrc && <img src={imageSrc} alt={text} />}
                {text}
            </button>
        );
    }

    const to = target ?? "#";
    return (
        <NavLink to={to} className={`button-component ${style ?? ""}`}>
            {icon}
            {imageSrc && <img src={imageSrc} alt={text} />}
            {text}
        </NavLink>
    );
}

export default Button;