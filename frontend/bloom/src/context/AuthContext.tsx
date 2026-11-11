import { createContext, useContext, useState, useEffect, useMemo, useCallback } from "react";
import type { ReactNode } from "react";
import { getMe, logout as logoutApi } from "../features/auth/api.ts";
import { syncStrava } from "../features/strava/api.ts";
import { fetchFromServer, onServerContact } from "../assets/js/data/apiClient.ts";
import { createSessionKeepAlive } from "../assets/js/data/sessionKeepAlive.ts";

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
                if (isMounted) {
                    setIsAuthenticated(true);
                    // Fire-and-forget: pull in any new Strava activities.
                    // The backend throttles repeat syncs, so this stays cheap.
                    syncStrava().catch(() => {});
                }
            } catch {
                if (isMounted) setIsAuthenticated(false);
            } finally {
                if (isMounted) setIsLoading(false);
            }
        };

        verifySession();

        return () => { isMounted = false; };
    }, []);

    // Keep the sliding session alive while the user works in the UI without
    // otherwise reaching the server; an idle tab still times out.
    useEffect(() => {
        if (!isAuthenticated) return;

        const keepAlive = createSessionKeepAlive(() => fetchFromServer<unknown>("users/me", "GET"));
        const unsubscribe = onServerContact(() => keepAlive.contact());
        const onActivity = () => keepAlive.activity();
        const events = ["pointerdown", "keydown", "wheel", "touchstart"] as const;
        events.forEach(e => window.addEventListener(e, onActivity, { passive: true }));

        return () => {
            unsubscribe();
            events.forEach(e => window.removeEventListener(e, onActivity));
        };
    }, [isAuthenticated]);

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
