import { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import { getMe } from "../features/auth/api.ts";

interface AuthContextType {
    token: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    setToken: (token: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setTokenState] = useState<string | null>(
        () => localStorage.getItem("jwt")
    );
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
    // Only start in a loading state if we have a token to verify
    const [isLoading, setIsLoading] = useState<boolean>(() => localStorage.getItem("jwt") !== null);

    const setToken = (newToken: string) => {
        localStorage.setItem("jwt", newToken);
        setTokenState(newToken);
    };

    const logout = () => {
        localStorage.removeItem("jwt");
        setTokenState(null);
        setIsAuthenticated(false);
    };

    useEffect(() => {
        if (!token) {
            // eslint-disable-next-line react-hooks/set-state-in-effect
            setIsLoading(false);
            return;
        }

        let isMounted = true;
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setIsLoading(true);

        const verifyToken = async () => {
            try {
                await getMe();
                if (isMounted) {
                    setIsAuthenticated(true);
                    setIsLoading(false);
                }
            } catch {
                if (isMounted) {
                    setIsAuthenticated(false);
                    localStorage.removeItem("jwt");
                    setTokenState(null);
                    setIsLoading(false);
                }
            }
        };

        verifyToken();

        return () => {
            isMounted = false;
        };
    }, [token]);

    return (
        <AuthContext.Provider value={{ token, isAuthenticated, isLoading, setToken, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextType {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within AuthProvider");
    }
    return context;
}
