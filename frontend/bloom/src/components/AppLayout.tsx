import { useEffect, type ReactNode } from "react";
import SidebarComponent from "./SidebarComponent.tsx";
import BottomNavComponent from "./BottomNavComponent.tsx";
import { searchExercises } from "../features/exercises/api.ts";

function AppLayout({ children }: { children: ReactNode }) {
    useEffect(() => { searchExercises(); }, []);
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
