import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";

interface PasswordInputProps {
    id: string;
    placeholder: string;
    value?: string;
    required?: boolean;
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

function PasswordInput({ id, placeholder, value, required, onChange }: PasswordInputProps) {
    const [visible, setVisible] = useState(false);

    return (
        <div className="password-input-wrapper">
            <input
                type={visible ? "text" : "password"}
                id={id}
                placeholder={placeholder}
                value={value}
                required={required}
                onChange={onChange}
            />
            <button
                type="button"
                className="password-toggle"
                onClick={() => setVisible(v => !v)}
                aria-label={visible ? "Hide password" : "Show password"}
            >
                {visible ? <EyeOff size={15} /> : <Eye size={15} />}
            </button>
        </div>
    );
}

export default PasswordInput;
