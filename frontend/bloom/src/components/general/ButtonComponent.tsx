import { NavLink } from 'react-router-dom';

interface ButtonComponentProps {
    text: string;
    imageSrc?: string;
    target?: string;
    style: string;

}


function Button({ text, imageSrc, target, style }: ButtonComponentProps) {
    const to = target ? target : "#";

    return (
        <NavLink
            to={to}
            className={`button-component ${style ?? ""}`}
        >
            {imageSrc && <img src={imageSrc} alt={text} />}
            {text}
        </NavLink>
    );
}

export default Button;