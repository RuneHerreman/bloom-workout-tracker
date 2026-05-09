import { createContext, useContext, useState } from "react";
import type { ReactNode } from "react";

interface AuthContextType {
    token: string | null;
    isAuthenticated: boolean;
    setToken: (token: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setTokenState] = useState<string | null>(
        () => localStorage.getItem("jwt")
    );

    const setToken = (newToken: string) => {
        localStorage.setItem("jwt", newToken);
        setTokenState(newToken);
    };

    const logout = () => {
        localStorage.removeItem("jwt");
        setTokenState(null);
    };

    return (
        <AuthContext.Provider value={{ token, isAuthenticated: token !== null, setToken, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth(): AuthContextType {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within AuthProvider");
    }
    return context;
}
