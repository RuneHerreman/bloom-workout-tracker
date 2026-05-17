import { createContext, useContext, useState } from "react";
import type { ReactNode } from "react";

interface DarkModeContextValue {
    dark: boolean;
    toggle: () => void;
}

const DarkModeContext = createContext<DarkModeContextValue | null>(null);

export function DarkModeProvider({ children }: { children: ReactNode }) {
    const [dark, setDark] = useState(() => localStorage.getItem("theme") === "dark");

    function toggle() {
        setDark(d => {
            const next = !d;
            localStorage.setItem("theme", next ? "dark" : "light");
            return next;
        });
    }

    return (
        <DarkModeContext.Provider value={{ dark, toggle }}>
            {children}
        </DarkModeContext.Provider>
    );
}

export function useDarkModeContext() {
    const ctx = useContext(DarkModeContext);
    if (!ctx) throw new Error("useDarkModeContext must be used inside DarkModeProvider");
    return ctx;
}
