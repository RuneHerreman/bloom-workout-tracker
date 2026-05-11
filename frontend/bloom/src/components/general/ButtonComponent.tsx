import { NavLink } from 'react-router-dom';

interface ButtonComponentProps {
    text: string;
    icon?: string;
    imageSrc?: string;
    target?: string;
    style: string;
}

function Button({ text, icon, imageSrc, target, style }: ButtonComponentProps) {
    const to = target ? target : "#";

    return (
        <NavLink
            to={to}
            className={`button-component ${style ?? ""}`}
        >
            {icon && <span aria-hidden="true">{icon}</span>}
            {imageSrc && <img src={imageSrc} alt={text} />}
            {text}
        </NavLink>
    );
}

export default Button;