import type { ReactNode } from "react";
import SidebarComponent from "./SidebarComponent.tsx";
import BottomNavComponent from "./BottomNavComponent.tsx";
import { useDarkMode } from "../hooks/useDarkMode.ts";

function AppLayout({ children }: { children: ReactNode }) {
    const { dark } = useDarkMode();

    return (
        <div className={`app-layout${dark ? " dark" : ""}`}>
            <SidebarComponent />
            <main className="app-main">
                {children}
            </main>
            <BottomNavComponent />
        </div>
    );
}

export default AppLayout;
