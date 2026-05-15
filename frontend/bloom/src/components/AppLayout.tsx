import { useState } from "react";
import type { ReactNode } from "react";
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
                        onClick={() => setSidebarOpen(true)}
                        aria-label="Open menu"
                    >
                        <span /><span /><span />
                    </button>
                    <img src="/media/bloom_logo.png" alt="Bloom" className="mobile-logo" />
                </div>
                {children}
            </main>
        </div>
    );
}

export default AppLayout;
