import { NavLink } from "react-router-dom";
import { useEffect, useState } from "react";
import { Moon, Sun, LogOut, LayoutDashboard, LayoutTemplate, BookOpen, Calculator, Dumbbell, TrendingUp, Trophy } from "lucide-react";
import "../assets/css/sidebar.css";
import { prefetchRoute } from "../routePrefetch.ts";
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

            <NavLink to="/dashboard" className={navClass} onPointerEnter={() => prefetchRoute("/dashboard")} onFocus={() => prefetchRoute("/dashboard")}><LayoutDashboard size={14}/> Dashboard</NavLink>
            <NavLink to="/templates" className={navClass} onPointerEnter={() => prefetchRoute("/templates")} onFocus={() => prefetchRoute("/templates")}><LayoutTemplate size={14}/> Templates</NavLink>
            <NavLink to="/logbook" className={navClass} onPointerEnter={() => prefetchRoute("/logbook")} onFocus={() => prefetchRoute("/logbook")}><BookOpen size={14}/> Log Book</NavLink>
            <NavLink to="/insights" className={navClass} onPointerEnter={() => prefetchRoute("/insights")} onFocus={() => prefetchRoute("/insights")}><TrendingUp size={14}/> Insights</NavLink>
            <NavLink to="/leaderboard" className={navClass} onPointerEnter={() => prefetchRoute("/leaderboard")} onFocus={() => prefetchRoute("/leaderboard")}><Trophy size={14}/> Leaderboard</NavLink>

            <span className="sidebar-section-label">Tools</span>
            <NavLink to="/tools/macro-calculator" className={navClass} onPointerEnter={() => prefetchRoute("/tools/macro-calculator")} onFocus={() => prefetchRoute("/tools/macro-calculator")}><Calculator size={14}/> Macro Calculator</NavLink>
            <NavLink to="/tools/one-rep-max" className={navClass} onPointerEnter={() => prefetchRoute("/tools/one-rep-max")} onFocus={() => prefetchRoute("/tools/one-rep-max")}><Dumbbell size={14}/> 1RM Calculator</NavLink>

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
