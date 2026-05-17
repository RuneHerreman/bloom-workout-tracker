import { NavLink } from "react-router-dom";
import { useEffect, useState } from "react";
import { Moon, Sun, LogOut, LayoutDashboard, LayoutTemplate, BookOpen, Calculator, Dumbbell, TrendingUp } from "lucide-react";
import "../assets/css/sidebar.css";
import { getMe } from "../features/auth/api.ts";
import type { User } from "../features/auth/api.ts";
import { useDarkModeContext } from "../context/DarkModeContext.tsx";
import { useAuth } from "../context/AuthContext.tsx";


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
            <NavLink to="/dashboard" className={navClass}><LayoutDashboard size={14}/> Dashboard</NavLink>
            <NavLink to="/templates" className={navClass}><LayoutTemplate size={14}/> Templates</NavLink>
            <NavLink to="/logbook" className={navClass}><BookOpen size={14}/> Log Book</NavLink>
            <NavLink to="/insights" className={navClass}><TrendingUp size={14}/> Insights</NavLink>

            <span className="sidebar-section-label">Tools</span>
            <NavLink to="/tools/macro-calculator" className={navClass}><Calculator size={14}/> Macro Calculator</NavLink>
            <NavLink to="/tools/one-rep-max" className={navClass}><Dumbbell size={14}/> 1RM Calculator</NavLink>

            <div className="sidebar-user">
                <NavLink to="/profile" className="sidebar-username">{user?.username ?? "—"}</NavLink>
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
