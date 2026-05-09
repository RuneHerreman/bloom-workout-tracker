import type { ReactNode } from "react";
import SidebarComponent from "./SidebarComponent.tsx";

function AppLayout({ children }: { children: ReactNode }) {
    return (
        <div className="app-layout">
            <SidebarComponent/>
            <main className="app-main">
                {children}
            </main>
        </div>
    );
}

export default AppLayout;
