import { createContext, useContext, useState, useEffect, useMemo, useCallback } from "react";
import type { ReactNode } from "react";
import { getMe, logout as logoutApi } from "../features/auth/api.ts";

interface AuthContextType {
    isAuthenticated: boolean;
    isLoading: boolean;
    markAuthenticated: () => void;
    logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const markAuthenticated = useCallback(() => setIsAuthenticated(true), []);

    const logout = useCallback(async () => {
        try {
            await logoutApi();
        } catch {
            // server unreachable — proceed anyway, cookie will expire naturally
        }
        window.location.href = "/login";
    }, []);

    useEffect(() => {
        let isMounted = true;

        const verifySession = async () => {
            try {
                await getMe();
                if (isMounted) setIsAuthenticated(true);
            } catch {
                if (isMounted) setIsAuthenticated(false);
            } finally {
                if (isMounted) setIsLoading(false);
            }
        };

        verifySession();

        return () => { isMounted = false; };
    }, []);

    const value = useMemo(
        () => ({ isAuthenticated, isLoading, markAuthenticated, logout }),
        [isAuthenticated, isLoading, markAuthenticated, logout]
    );

    return (
        <AuthContext.Provider value={value}>
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
