import { useState } from "react";
import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import SidebarComponent from "./SidebarComponent.tsx";

function AppLayout({ children }: { children: ReactNode }) {
    const [sidebarOpen, setSidebarOpen] = useState(false);

    return (
        <div className="app-layout">
            <div
                className={`sidebar-backdrop${sidebarOpen ? " open" : ""}`}
                onClick={() => setSidebarOpen(false)}
            />
            <SidebarComponent isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
            <main className="app-main">
                <div className="mobile-header">
                    <button
                        className="burger-btn"
                        onClick={() => setSidebarOpen(v => !v)}
                        aria-label="Open menu"
                    >
                        <span /><span /><span />
                    </button>
                    <NavLink to="/dashboard"><img src="/media/bloom_logo.png" alt="Bloom" className="mobile-logo" /></NavLink>
                </div>
                {children}
            </main>
        </div>
    );
}

export default AppLayout;
