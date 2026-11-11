import { useEffect } from "react";
import type { ReactNode } from "react";
import SidebarComponent from "./SidebarComponent.tsx";
import BottomNavComponent from "./BottomNavComponent.tsx";
import { DarkModeProvider, useDarkModeContext } from "../context/DarkModeContext.tsx";
import { ToastProvider } from "../context/ToastContext.tsx";
import { prefetchAppInBackground } from "../routePrefetch.ts";

function AppLayoutInner({ children }: { children: ReactNode }) {
    const { dark } = useDarkModeContext();

    useEffect(() => {
        document.documentElement.classList.toggle("dark", dark);
    }, [dark]);

    useEffect(() => { prefetchAppInBackground(); }, []);

    return (
        <div className="app-layout">
            <SidebarComponent />
            <main className="app-main">
                {children}
            </main>
            <BottomNavComponent />
        </div>
    );
}

function AppLayout({ children }: { children: ReactNode }) {
    return (
        <DarkModeProvider>
            <ToastProvider>
                <AppLayoutInner>{children}</AppLayoutInner>
            </ToastProvider>
        </DarkModeProvider>
    );
}

export default AppLayout;
