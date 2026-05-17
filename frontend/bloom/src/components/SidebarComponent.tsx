import { NavLink } from "react-router-dom";
import { useEffect, useState } from "react";
import { Moon, Sun, LogOut } from "lucide-react";
import "../assets/css/sidebar.css";
import { getMe } from "../features/auth/api.ts";
import type { User } from "../features/auth/api.ts";
import { useDarkModeContext } from "../context/DarkModeContext.tsx";
import { useAuth } from "../context/AuthContext.tsx";

const GridIcon = () => (
    <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor" aria-hidden="true">
        <rect x="0" y="0" width="6" height="6" rx="1"/>
        <rect x="8" y="0" width="6" height="6" rx="1"/>
        <rect x="0" y="8" width="6" height="6" rx="1"/>
        <rect x="8" y="8" width="6" height="6" rx="1"/>
    </svg>
);

function navClass({ isActive }: { isActive: boolean }) {
    return `sidebar-item${isActive ? " active" : ""}`;
}

function SidebarComponent() {
    const [user, setUser] = useState<User | null>(null);
    const { dark, toggle } = useDarkModeContext();
    const { logout } = useAuth();

    useEffect(() => {
        getMe().then(setUser).catch(() => null);
    }, []);

    return (
        <aside className="sidebar">
            <NavLink to="/dashboard" className="sidebar-logo">
                <img src="/media/bloom_logo.png" alt="Bloom"/>
            </NavLink>

            <span className="sidebar-section-label">Pages</span>
            <NavLink to="/dashboard" className={navClass}><GridIcon/> Dashboard</NavLink>
            <NavLink to="/templates" className={navClass}><GridIcon/> Templates</NavLink>
            <NavLink to="/logbook" className={navClass}><GridIcon/> Log Book</NavLink>

            <span className="sidebar-section-label">Tools</span>
            <NavLink to="/tools/macro-calculator" className={navClass}><GridIcon/> Macro Calculator</NavLink>
            <NavLink to="/tools/one-rep-max" className={navClass}><GridIcon/> 1RM Calculator</NavLink>

            <div className="sidebar-user">
                <span className="sidebar-username">{user?.username ?? "—"}</span>
                <button className="sidebar-theme-toggle" onClick={toggle} aria-label="Toggle dark mode">
                    {dark ? <Sun size={14} /> : <Moon size={14} />}
                </button>
                <button className="sidebar-logout" onClick={logout} aria-label="Log out">
                    <LogOut size={14} />
                </button>
            </div>
        </aside>
    );
}

export default SidebarComponent;
