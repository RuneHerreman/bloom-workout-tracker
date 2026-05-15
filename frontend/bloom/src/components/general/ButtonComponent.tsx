import { NavLink } from 'react-router-dom';

interface ButtonComponentProps {
    text: string;
    icon?: string;
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
                {icon && <span aria-hidden="true">{icon}</span>}
                {imageSrc && <img src={imageSrc} alt={text} />}
                {text}
            </button>
        );
    }

    const to = target ?? "#";
    return (
        <NavLink to={to} className={`button-component ${style ?? ""}`}>
            {icon && <span aria-hidden="true">{icon}</span>}
            {imageSrc && <img src={imageSrc} alt={text} />}
            {text}
        </NavLink>
    );
}

export default Button;