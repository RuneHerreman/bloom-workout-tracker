import { useState } from "react";

export function useDarkMode() {
    const [dark, setDark] = useState(() => localStorage.getItem("theme") === "dark");

    function toggle() {
        setDark(d => {
            const next = !d;
            localStorage.setItem("theme", next ? "dark" : "light");
            return next;
        });
    }

    return { dark, toggle };
}
