import { NavLink } from 'react-router-dom';

interface ButtonComponentProps {
    text: string;
    icon?: string;
    imageSrc?: string;
    target?: string;
    style: string;
    onClick?: () => void;
}

function Button({ text, icon, imageSrc, target, style, onClick }: ButtonComponentProps) {
    if (onClick) {
        return (
            <button type="button" className={`button-component ${style ?? ""}`} onClick={onClick}>
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