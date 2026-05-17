import { useEffect } from "react";
import type { ReactNode } from "react";
import SidebarComponent from "./SidebarComponent.tsx";
import BottomNavComponent from "./BottomNavComponent.tsx";
import { DarkModeProvider, useDarkModeContext } from "../context/DarkModeContext.tsx";

function AppLayoutInner({ children }: { children: ReactNode }) {
    const { dark } = useDarkModeContext();

    useEffect(() => {
        document.documentElement.classList.toggle("dark", dark);
    }, [dark]);

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
            <AppLayoutInner>{children}</AppLayoutInner>
        </DarkModeProvider>
    );
}

export default AppLayout;
