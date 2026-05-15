import type { ReactNode } from "react";
import SidebarComponent from "./SidebarComponent.tsx";
import BottomNavComponent from "./BottomNavComponent.tsx";

function AppLayout({ children }: { children: ReactNode }) {
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

export default AppLayout;
